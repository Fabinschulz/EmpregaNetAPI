using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// <c>Cities</c> do vocabulário do feed (<c>emp-filtros-vagas-localizacao</c>): cidades distintas das
/// vagas <b>visíveis no catálogo</b>, agrupadas pelo código da UF — a mesma regra de visibilidade do
/// feed, para que o filtro nunca ofereça uma cidade cujas vagas o feed esconde.
/// </summary>
/// <remarks>
/// <para>Integração porque a regra está na consulta de <c>JobRepository.GetActiveCitiesByStateAsync</c>
/// (join com empresa, filtro de visibilidade, <c>Trim</c> + <c>Distinct</c>), não no handler.</para>
///
/// <para>Cada teste usa uma BD InMemory <b>própria</b>, e não a do <see cref="InMemoryIdentityFixture"/>:
/// o que se afirma é o conjunto inteiro de <c>Cities</c> ("UF sem vaga não aparece"), e a BD partilhada
/// contém vagas criadas por outros testes da coleção. Não há Identity envolvido.</para>
///
/// <para><b>Limitação do provider InMemory:</b> <c>Trim</c>, <c>Distinct</c> sobre o tipo anônimo e o join
/// correm em memória. A tradução para o Npgsql (<c>btrim</c>, <c>DISTINCT</c> sobre o owned type
/// <c>Location</c>) não é provada aqui — fica para o E2E contra a API de desenvolvimento.</para>
/// </remarks>
[Collection("Integration")]
public sealed class GetJobVocabularyHandlerIntegrationTests : IDisposable
{
    private readonly PostgreSqlContext _context = new(
        new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseInMemoryDatabase($"vocabulary_{Guid.NewGuid():N}")
            .Options);

    public void Dispose() => _context.Dispose();

    // ---------- Contrato: agrupamento, distinct/trim e ordenação ----------

    [Fact]
    public async Task Handle_Contrato_DeveAgruparPorCodigoDeUfComCidadesDistintasEOrdenadas()
    {
        // Arrange: inseridas fora de ordem, com repetição e espaços.
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.SP, "São Paulo");
        await CreateJobAsync(company, UF.CE, "Fortaleza");
        await CreateJobAsync(company, UF.SP, "Campinas");
        await CreateJobAsync(company, UF.CE, "Fortaleza");     // mesma cidade, duas vagas
        await CreateJobAsync(company, UF.CE, " Fortaleza ");   // espaços colapsam
        await CreateJobAsync(company, UF.CE, "Caucaia");

        // Act
        var result = await HandleAsync();

        // Assert
        result.Cities.Should().SatisfyRespectively(
            ce =>
            {
                ce.State.Should().Be("CE");
                ce.Items.Should().Equal("Caucaia", "Fortaleza");
            },
            sp =>
            {
                sp.State.Should().Be("SP");
                sp.Items.Should().Equal("Campinas", "São Paulo");
            });
    }

    [Fact]
    public async Task Handle_Contrato_UfSemVagaNaoDeveAparecer()
    {
        // Arrange
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");

        // Act
        var result = await HandleAsync();

        // Assert
        result.Cities.Should().ContainSingle().Which.State.Should().Be("MG");
    }

    [Fact]
    public async Task Handle_Contrato_SemVagasVisiveis_CitiesDeveVirVazioENaoNulo()
    {
        var result = await HandleAsync();

        result.Cities.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Contrato_CamposEstaticosDevemContinuarPreenchidos()
    {
        // A parte estática vem de static readonly; Cities é por requisição e não pode apagá-la.
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");

        var result = await HandleAsync();

        result.States.Should().NotBeEmpty();
        result.JobTypes.Should().NotBeEmpty();
        result.Requirements.Should().NotBeEmpty();
        result.Benefits.Should().NotBeEmpty();
        result.MaxItemsPerJob.Should().BeGreaterThan(0);
    }

    // ---------- CA-08: vaga fora do catálogo não gera cidade ----------

    [Fact]
    public async Task Handle_CA08_VagaInativa_NaoDeveGerarCidade()
    {
        // Arrange
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");
        await CreateJobAsync(company, UF.AM, "Manaus", configure: job => job.Close());

        // Act
        var result = await HandleAsync();

        // Assert
        AssertOnlyExtrema(result);
    }

    [Fact]
    public async Task Handle_CA08_VagaExcluidaLogicamente_NaoDeveGerarCidade()
    {
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");
        await CreateJobAsync(company, UF.BA, "Salvador", configure: job =>
        {
            job.IsDeleted = true;
            job.DeletedAt = DateTimeOffset.UtcNow;
        });

        var result = await HandleAsync();

        AssertOnlyExtrema(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_CA08_VagaSemCidade_NaoDeveGerarEntrada(string city)
    {
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");
        await CreateJobAsync(company, UF.PA, city);

        var result = await HandleAsync();

        AssertOnlyExtrema(result);
    }

    [Fact]
    public async Task Handle_CA08_VagaComUfNaoSelecionada_NaoDeveGerarEntrada()
    {
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");
        await CreateJobAsync(company, UF.NaoSelecionado, "Cidade Sem Estado");

        var result = await HandleAsync();

        AssertOnlyExtrema(result);
    }

    // Mesma regra de visibilidade do feed: a vaga está ativa, mas a empresa foi excluída logicamente,
    // e o feed não a mostra — oferecer a cidade levaria a um filtro sem resultado.
    [Fact]
    public async Task Handle_CA08_VagaAtivaDeEmpresaExcluidaLogicamente_NaoDeveGerarCidade()
    {
        // Arrange
        var activeCompany = await CreateCompanyAsync();
        var deletedCompany = await CreateCompanyAsync(deleted: true);
        await CreateJobAsync(activeCompany, UF.MG, "Extrema");
        await CreateJobAsync(deletedCompany, UF.RS, "Porto Alegre");

        // Act
        var result = await HandleAsync();

        // Assert
        AssertOnlyExtrema(result);
    }

    [Fact]
    public async Task Handle_CA08_CidadeComVagaVisivelEOutraOculta_DeveAparecerUmaVez()
    {
        // A vaga oculta não apaga a cidade que outra vaga visível mantém no catálogo.
        var company = await CreateCompanyAsync();
        await CreateJobAsync(company, UF.MG, "Extrema");
        await CreateJobAsync(company, UF.MG, "Extrema", configure: job => job.Close());

        var result = await HandleAsync();

        AssertOnlyExtrema(result);
    }

    // ---------- Infraestrutura do teste ----------

    private static void AssertOnlyExtrema(JobVocabularyViewModel result)
    {
        var group = result.Cities.Should().ContainSingle().Subject;
        group.State.Should().Be("MG");
        group.Items.Should().Equal("Extrema");
    }

    private Task<JobVocabularyViewModel> HandleAsync() =>
        new GetJobVocabularyHandler(new JobRepository(_context))
            .Handle(new GetJobVocabularyQuery(), CancellationToken.None);

    private async Task<long> CreateCompanyAsync(bool deleted = false)
    {
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
            },
            IsDeleted = deleted,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company.Id;
    }

    private async Task CreateJobAsync(long companyId, UF state, string city, Action<Job>? configure = null)
    {
        var job = new Job(
            companyId: companyId,
            title: $"Operador {Guid.NewGuid():N}",
            description: "Linha de produção.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = city, State = state },
            salaryMin: 2100m);

        configure?.Invoke(job);

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
    }
}
