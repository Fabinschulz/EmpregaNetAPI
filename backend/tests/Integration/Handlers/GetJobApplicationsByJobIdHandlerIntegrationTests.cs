using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Queries;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// O que o recrutador vê na lista da vaga. O ponto é a <b>autoria do cancelamento</b>: a desistência
/// do candidato e o descarte pela empresa são dois status distintos, e a distinção tem de sobreviver
/// à leitura — é o que permite à tela dizer "cancelada pelo candidato" sem inventar a informação.
/// </summary>
/// <remarks>
/// Limitação do provider InMemory: não reproduz constraints nem semântica do PostgreSQL.
/// </remarks>
[Collection("Integration")]
public sealed class GetJobApplicationsByJobIdHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public GetJobApplicationsByJobIdHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Job CreateJob(long companyId) => new(
        companyId: companyId,
        title: $"Auxiliar de Produção {Guid.NewGuid():N}",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.AteUmAno,
        area: JobAreaEnum.Logistica,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        salaryMin: 2100m);

    private static GetJobApplicationsByJobIdHandler CreateSut(PostgreSqlContext context)
    {
        var employerAccess = new Mock<IJobEmployerAccess>();
        employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new GetJobApplicationsByJobIdHandler(
            new JobRepository(context),
            new JobApplicationRepository(context),
            employerAccess.Object,
            NullLogger<GetJobApplicationsByJobIdHandler>.Instance);
    }

    private static async Task SeedAsync(PostgreSqlContext context, long jobId, long userId, ApplicationStatusEnum status)
    {
        var repository = new JobApplicationRepository(context);
        var application = new JobApplication(jobId, userId);
        await repository.CreateAsync(application, CancellationToken.None);

        if (status == ApplicationStatusEnum.CanceledByCandidate)
        {
            application.CancelByCandidate();
            await repository.UpdateAsync(application, CancellationToken.None);
        }
        else if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
            await repository.UpdateAsync(application, CancellationToken.None);
        }
    }

    [Fact]
    public async Task Handle_VagaComOsDoisCancelamentos_DeveDistinguirAAutoria()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7401);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        await SeedAsync(context, job.Id, userId: 7411, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, job.Id, userId: 7412, ApplicationStatusEnum.Canceled);
        await SeedAsync(context, job.Id, userId: 7413, ApplicationStatusEnum.Pending);

        var result = await CreateSut(context)
            .Handle(new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, null, null), CancellationToken.None);

        result.Data.Should().HaveCount(3);
        result.Data.Should().ContainSingle(a => a.Candidate.Id == 7411)
            .Which.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        result.Data.Should().ContainSingle(a => a.Candidate.Id == 7412)
            .Which.Status.Should().Be(ApplicationStatusEnum.Canceled);
    }

    [Fact]
    public async Task Handle_FiltroCanceladaPeloCandidato_NaoDeveTrazerOCancelamentoDaEmpresa()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7402);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        await SeedAsync(context, job.Id, userId: 7421, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, job.Id, userId: 7422, ApplicationStatusEnum.Canceled);

        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, "CanceledByCandidate", null),
            CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data[0].Candidate.Id.Should().Be(7421);
    }

    // ---------- emp-filtros-candidaturas-recrutamento — CA-04: busca por candidato na vaga ----------

    [Fact]
    public async Task Handle_CA04_BuscaPeloNomeDoCandidato_DeveFiltrarAntesDaPaginacao()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var name = Token("nomevaga");
        var job = CreateJob(companyId: 7403);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var match = await CreateCandidateAsync(usernamePrefix: name);
        var other = await CreateCandidateAsync();
        await SeedAsync(context, job.Id, match, ApplicationStatusEnum.Pending);
        await SeedAsync(context, job.Id, other, ApplicationStatusEnum.Pending);

        // Act
        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, null, null, name.ToUpperInvariant()),
            CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(match);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA04_BuscaPeloEmailDoCandidato_DeveFiltrar()
    {
        // Arrange: o termo está só no e-mail.
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var mailbox = Token("mailvaga");
        var job = CreateJob(companyId: 7404);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var match = await CreateCandidateAsync(email: $"{mailbox}@test.local");
        var other = await CreateCandidateAsync();
        await SeedAsync(context, job.Id, match, ApplicationStatusEnum.Pending);
        await SeedAsync(context, job.Id, other, ApplicationStatusEnum.Pending);

        // Act
        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, null, null, mailbox),
            CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(match);
    }

    [Fact]
    public async Task Handle_CA04_BuscaCombinadaComStatus_DeveAplicarOsDois()
    {
        // Arrange: dois candidatos casam a busca, só um está no status pedido.
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var name = Token("combovaga");
        var job = CreateJob(companyId: 7405);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var pendingMatch = await CreateCandidateAsync(usernamePrefix: name);
        var processingMatch = await CreateCandidateAsync(usernamePrefix: name);
        var processingOther = await CreateCandidateAsync();
        await SeedAsync(context, job.Id, pendingMatch, ApplicationStatusEnum.Pending);
        await SeedAsync(context, job.Id, processingMatch, ApplicationStatusEnum.Processing);
        await SeedAsync(context, job.Id, processingOther, ApplicationStatusEnum.Processing);

        // Act
        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, "Processing", null, name),
            CancellationToken.None);

        // Assert
        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(processingMatch);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA04_BuscaSemCorrespondencia_DeveDevolverTotalZero()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7406);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);
        await SeedAsync(context, job.Id, await CreateCandidateAsync(), ApplicationStatusEnum.Pending);

        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, null, null, Token("naocasa")),
            CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    // Mesmo parser e mesmo DomainError do endpoint geral: "99" passaria no Enum.TryParse e filtraria
    // em silêncio para uma lista vazia em vez de 400.
    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("NaoSelecionado")]
    [InlineData("Inexistente")]
    [InlineData("Foo")]
    [InlineData("1")]                // número de membro declarado: só o nome é aceito
    [InlineData("Approved,Pending")] // lista com vírgula: Enum.TryParse faria OU bit a bit (= Rejected)
    public async Task Handle_StatusInvalido_DeveLancarInvalidQueryFilter(string status)
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7407);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var act = async () => await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, status, null),
            CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.INVALID_QUERY_FILTER);
    }

    [Fact]
    public async Task Handle_StatusComEspacosNasPontasECaixaDiferente_DeveSerAceito()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7408);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);
        await SeedAsync(context, job.Id, userId: 7481, ApplicationStatusEnum.Pending);
        await SeedAsync(context, job.Id, userId: 7482, ApplicationStatusEnum.Processing);

        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, " processing ", null),
            CancellationToken.None);

        result.Data.Should().ContainSingle().Which.Candidate.Id.Should().Be(7482);
    }

    private static string Token(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..(prefix.Length + 12)];

    private async Task<long> CreateCandidateAsync(string? usernamePrefix = null, string? email = null) =>
        await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fixture.Services,
            email ?? TestDataFactory.UniqueEmail("jobcand"),
            usernamePrefix ?? "jobcand");
}
