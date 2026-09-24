using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Queries;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Application.Jobs.UseCase;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// <c>GET /api/jobapplications</c> (tela <c>/recrutamento/candidaturas</c>): filtro de status, busca
/// por candidato/vaga e escopo de empresa compostos na mesma consulta, antes da paginação.
/// </summary>
/// <remarks>
/// <para>Integração porque status, busca e escopo vivem na consulta do repositório (join de
/// candidatura, vaga e candidato), não no handler. O escopo é resolvido pelo
/// <see cref="JobEmployerAccess"/> real sobre o Identity do fixture: o recrutador enxerga a própria
/// empresa porque <c>User.EmployerCompanyId</c> persistido diz isso, não porque um duplo o afirma.</para>
///
/// <para>A BD InMemory é partilhada pela coleção, por isso cada cenário usa termos únicos (nome de
/// usuário, e-mail, título) e uma empresa própria: é o que torna <c>TotalItems</c> determinístico.</para>
///
/// <para><b>Limitação do provider InMemory:</b> aceita qualquer LINQ e compara em memória. A tradução
/// da busca (<c>ToLower().Contains</c> sobre o join) para o Npgsql não é provada aqui — isso é o E2E
/// contra a API de desenvolvimento.</para>
/// </remarks>
[Collection("Integration")]
public sealed class GetAllJobApplicationsHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public GetAllJobApplicationsHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    // ---------- CA-02: status restringe a lista, combinado com busca e escopo ----------

    [Fact]
    public async Task Handle_CA02_FiltroDeStatus_DeveDevolverSoAsCandidaturasNoStatus()
    {
        // Arrange
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var pending = await CreateCandidateAsync();
        var processing = await CreateCandidateAsync();
        await ApplyAsync(job.Id, pending.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(job.Id, processing.Id, ApplicationStatusEnum.Processing);

        // Act
        var result = await HandleAsync(recruiterId, Query(status: "Processing"));

        // Assert
        result.Data.Should().ContainSingle()
            .Which.Candidate.Id.Should().Be(processing.Id);
        result.Data.Should().OnlyContain(a => a.Status == ApplicationStatusEnum.Processing);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA02_StatusComBuscaEEscopoDeEmpresa_DeveAplicarOsTresJuntos()
    {
        // Arrange: o mesmo termo casa candidaturas nas duas empresas e nos dois status.
        var term = Token("combo");
        var ownCompanyId = await CreateCompanyAsync();
        var otherCompanyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(ownCompanyId);

        var ownJob = await CreateJobAsync(ownCompanyId, $"Operador {term}");
        var otherJob = await CreateJobAsync(otherCompanyId, $"Operador {term}");
        var unrelatedOwnJob = await CreateJobAsync(ownCompanyId);

        var expected = await CreateCandidateAsync();
        var wrongStatus = await CreateCandidateAsync();
        var otherCompany = await CreateCandidateAsync();
        var noMatch = await CreateCandidateAsync();

        await ApplyAsync(ownJob.Id, expected.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(ownJob.Id, wrongStatus.Id, ApplicationStatusEnum.Processing);
        await ApplyAsync(otherJob.Id, otherCompany.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(unrelatedOwnJob.Id, noMatch.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(status: "Pending", search: term));

        // Assert
        result.Data.Should().ContainSingle()
            .Which.Candidate.Id.Should().Be(expected.Id);
        result.TotalItems.Should().Be(1);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("PENDING")]
    public async Task Handle_CA02_StatusEmQualquerCaixa_DeveSerAceito(string status)
    {
        // Arrange
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var candidate = await CreateCandidateAsync();
        await ApplyAsync(job.Id, candidate.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(status: status));

        // Assert
        result.Data.Should().ContainSingle().Which.Status.Should().Be(ApplicationStatusEnum.Pending);
    }

    [Fact]
    public async Task Handle_CA02_StatusComEspacosNasPontasECaixaDiferente_DeveSerAceito()
    {
        // Arrange
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var pending = await CreateCandidateAsync();
        var processing = await CreateCandidateAsync();
        await ApplyAsync(job.Id, pending.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(job.Id, processing.Id, ApplicationStatusEnum.Processing);

        // Act
        var result = await HandleAsync(recruiterId, Query(status: " processing "));

        // Assert
        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(processing.Id);
    }

    // Regressão do spec: o endpoint geral recusa com o mesmo DomainError do endpoint por vaga.
    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("NaoSelecionado")]
    [InlineData("Recebida")]
    [InlineData("Inexistente")]
    [InlineData("Foo")]
    [InlineData("1")]                // número de membro declarado: só o nome é aceito
    [InlineData("Approved,Pending")] // lista com vírgula: Enum.TryParse faria OU bit a bit (= Rejected)
    public async Task Handle_CA02_StatusInvalido_DeveLancarInvalidQueryFilter(string status)
    {
        // Arrange
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);

        // Act
        var act = async () => await HandleAsync(recruiterId, Query(status: status));

        // Assert
        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.INVALID_QUERY_FILTER);
    }

    // ---------- CA-03: busca por nome, e-mail do candidato ou título da vaga ----------

    [Fact]
    public async Task Handle_CA03_BuscaPeloNomeDoCandidato_SemDistinguirCaixa()
    {
        // Arrange
        var name = Token("nome");
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var match = await CreateCandidateAsync(usernamePrefix: name);
        var other = await CreateCandidateAsync();
        await ApplyAsync(job.Id, match.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(job.Id, other.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: $"  {name.ToUpperInvariant()} "));

        // Assert
        result.Data.Should().ContainSingle()
            .Which.Candidate.Name.Should().Contain(name);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA03_BuscaPeloEmailDoCandidato_SemDistinguirCaixa()
    {
        // Arrange: o termo está só no e-mail, não no nome de usuário.
        var mailbox = Token("mail");
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var match = await CreateCandidateAsync(email: $"{mailbox}@test.local");
        var other = await CreateCandidateAsync();
        await ApplyAsync(job.Id, match.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(job.Id, other.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: mailbox.ToUpperInvariant()));

        // Assert
        var single = result.Data.Should().ContainSingle().Subject;
        single.Candidate.Id.Should().Be(match.Id);
        single.Candidate.Name.Should().NotContain(mailbox);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA03_BuscaPeloTituloDaVaga_SemDistinguirCaixa()
    {
        // Arrange
        var title = Token("Soldador");
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var matchingJob = await CreateJobAsync(companyId, $"Vaga de {title}");
        var otherJob = await CreateJobAsync(companyId);
        var first = await CreateCandidateAsync();
        var second = await CreateCandidateAsync();
        var elsewhere = await CreateCandidateAsync();
        await ApplyAsync(matchingJob.Id, first.Id, ApplicationStatusEnum.Pending);
        await ApplyAsync(matchingJob.Id, second.Id, ApplicationStatusEnum.Processing);
        await ApplyAsync(otherJob.Id, elsewhere.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: title.ToLowerInvariant()));

        // Assert
        result.Data.Select(a => a.Candidate.Id).Should().BeEquivalentTo(new[] { first.Id, second.Id });
        result.Data.Should().OnlyContain(a => a.JobId == matchingJob.Id);
        result.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task Handle_CA03_BuscaSemCorrespondencia_DeveDevolverTotalZero()
    {
        // Arrange
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId);
        var candidate = await CreateCandidateAsync();
        await ApplyAsync(job.Id, candidate.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: Token("naocasa")));

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CA03_TotalItemsDeveRefletirABuscaENaoAPagina()
    {
        // Arrange: três candidaturas casam o termo; a página tem tamanho 2.
        var title = Token("Pagina");
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var matchingJob = await CreateJobAsync(companyId, title);
        var otherJob = await CreateJobAsync(companyId);
        for (var i = 0; i < 3; i++)
        {
            var candidate = await CreateCandidateAsync();
            await ApplyAsync(matchingJob.Id, candidate.Id, ApplicationStatusEnum.Pending);
        }

        var outsider = await CreateCandidateAsync();
        await ApplyAsync(otherJob.Id, outsider.Id, ApplicationStatusEnum.Pending);

        // Act
        var page1 = await HandleAsync(recruiterId, Query(search: title, page: 1, size: 2));
        var page2 = await HandleAsync(recruiterId, Query(search: title, page: 2, size: 2));

        // Assert
        page1.TotalItems.Should().Be(3);
        page1.Data.Should().HaveCount(2);
        page2.TotalItems.Should().Be(3);
        page2.Data.Should().ContainSingle();
        page1.Data.Concat(page2.Data).Should().OnlyContain(a => a.JobId == matchingJob.Id);
    }

    // ---------- Regressão: escopo de empresa e LEFT JOIN ----------

    [Fact]
    public async Task Handle_Regressao_RecrutadorBuscandoNomeExatoDeCandidatoDeOutraEmpresa_NaoDeveEnxergar()
    {
        // Arrange: o candidato só se candidatou a vaga de outra empresa.
        var ownCompanyId = await CreateCompanyAsync();
        var otherCompanyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(ownCompanyId);
        var otherJob = await CreateJobAsync(otherCompanyId);
        var foreign = await CreateCandidateAsync();
        await ApplyAsync(otherJob.Id, foreign.Id, ApplicationStatusEnum.Pending);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: foreign.UserName));

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Regressao_RecrutadorDaOutraEmpresa_DeveEnxergarAMesmaCandidatura()
    {
        // Contraprova do isolamento: a candidatura existe e é encontrada por quem tem o escopo certo.
        var otherCompanyId = await CreateCompanyAsync();
        var otherRecruiterId = await CreateRecruiterAsync(otherCompanyId);
        var otherJob = await CreateJobAsync(otherCompanyId);
        var candidate = await CreateCandidateAsync();
        await ApplyAsync(otherJob.Id, candidate.Id, ApplicationStatusEnum.Pending);

        var result = await HandleAsync(otherRecruiterId, Query(search: candidate.UserName));

        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(candidate.Id);
    }

    [Fact]
    public async Task Handle_Regressao_CandidatoExcluidoLogicamente_DeveContinuarListadoEMarcado()
    {
        // Arrange
        var title = Token("Excluido");
        var companyId = await CreateCompanyAsync();
        var recruiterId = await CreateRecruiterAsync(companyId);
        var job = await CreateJobAsync(companyId, title);
        var deleted = await CreateCandidateAsync();
        await ApplyAsync(job.Id, deleted.Id, ApplicationStatusEnum.Pending);
        await SoftDeleteUserAsync(deleted.Id);

        // Act
        var result = await HandleAsync(recruiterId, Query(search: title));

        // Assert
        var single = result.Data.Should().ContainSingle().Subject;
        single.Candidate.Id.Should().Be(deleted.Id);
        single.Candidate.IsDeleted.Should().BeTrue();
        single.Candidate.Name.Should().Be(deleted.UserName);
    }

    // ---------- Infraestrutura do teste ----------

    private static GetAllJobApplicationsQuery Query(
        string? status = null, string? search = null, int page = 1, int size = 100) =>
        new(Page: page, Size: size, OrderBy: null, IsDeleted: null, Search: search, Status: status);

    private static string Token(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..(prefix.Length + 12)];

    private async Task<ListDataPagination<JobApplicationViewModel>> HandleAsync(
        long currentUserId, GetAllJobApplicationsQuery query)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(currentUserId);

        var sut = new GetAllJobApplicationsHandler(
            new JobApplicationRepository(context),
            new JobEmployerAccess(users, currentUser.Object),
            NullLogger<GetAllJobApplicationsHandler>.Instance);

        return await sut.Handle(query, CancellationToken.None);
    }

    private async Task<(long Id, string UserName)> CreateCandidateAsync(
        string? usernamePrefix = null, string? email = null)
    {
        var prefix = usernamePrefix ?? "cand";
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fixture.Services,
            email ?? TestDataFactory.UniqueEmail("ja"),
            prefix);

        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(id.ToString());
        return (id, user!.UserName!);
    }

    private async Task<long> CreateRecruiterAsync(long companyId)
    {
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fixture.Services, TestDataFactory.UniqueEmail("rec"), "rec");

        using var scope = _fixture.Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        if (!await roles.RoleExistsAsync(RecruitmentRoleNames.Recruiter))
        {
            await roles.CreateAsync(new Role { Name = RecruitmentRoleNames.Recruiter, DataInclusao = DateTimeOffset.UtcNow });
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(id.ToString());
        await users.AddToRoleAsync(user!, RecruitmentRoleNames.Recruiter);
        user!.UserType = UserTypeEnum.Recruiter;
        user.EmployerCompanyId = companyId;
        await users.UpdateAsync(user);

        return id;
    }

    private async Task SoftDeleteUserAsync(long userId)
    {
        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(userId.ToString());
        user!.IsDeleted = true;
        user.DeletedAt = DateTimeOffset.UtcNow;
        await users.UpdateAsync(user);
    }

    private async Task<long> CreateCompanyAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var company = new Company
        {
            CompanyName = $"Empresa {Guid.NewGuid():N}",
            RegistrationNumber = Random.Shared.NextInt64(10_000_000_000_000, 99_999_999_999_999).ToString(),
            Email = TestDataFactory.UniqueEmail("company"),
            Phone = TestDataFactory.UniqueBrazilianCell(),
            TypeOfActivity = TypeOfActivityEnum.Industry,
            Address = new Address
            {
                Street = "Rua Teste",
                Number = "100",
                Neighborhood = "Centro",
                City = "Extrema",
                State = UF.MG,
                ZipCode = "37640-000"
            }
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();
        return company.Id;
    }

    private async Task<Job> CreateJobAsync(long companyId, string? title = null)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = new Job(
            companyId: companyId,
            title: title ?? $"Auxiliar {Guid.NewGuid():N}",
            description: "Linha de montagem.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            salaryMin: 2100m);

        await new JobRepository(context).CreateAsync(job, CancellationToken.None);
        return job;
    }

    private async Task ApplyAsync(long jobId, long userId, ApplicationStatusEnum status)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        var repository = new JobApplicationRepository(context);

        var application = new JobApplication(jobId, userId);
        await repository.CreateAsync(application, CancellationToken.None);

        if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
            await repository.UpdateAsync(application, CancellationToken.None);
        }
    }
}
