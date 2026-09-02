using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
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
/// Recandidatura depois de desistir: a duplicata é aferida sobre candidaturas <b>activas</b>, e a
/// cancelada pelo candidato deixa de contar. O que continua a valer é a vaga: encerrada, recusa.
/// </summary>
/// <remarks>
/// Cenário de integração porque a regra está na consulta (<c>ExistsActiveAsync</c>), não no handler:
/// um repositório mockado devolveria o que o teste mandasse e não provaria nada.
///
/// <para>
/// <b>Limite honesto deste ficheiro.</b> O provider InMemory não aplica índices nem constraints, por
/// isso o teste da recandidatura passa aqui mesmo que o índice único de <c>(JobId, UserId)</c> a
/// proibisse no PostgreSQL — foi assim que o defeito passou despercebido. O que garante que a segunda
/// linha é sequer permitida no banco real é o índice <b>parcial</b>, verificado ao nível do modelo em
/// <c>Unit/Persistence/JobApplicationIndexTests</c>. Nenhum dos dois substitui uma base real.
/// </para>
/// </remarks>
[Collection("Integration")]
public sealed class ApplyToJobHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public ApplyToJobHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Job CreateJob(long companyId) => new(
        companyId: companyId,
        title: $"Operador(a) de Empilhadeira {Guid.NewGuid():N}",
        description: "Movimentação de cargas.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.SegundoTurno,
        experienceLevel: ExperienceLevelEnum.AteUmAno,
        area: JobAreaEnum.Logistica,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        salaryMin: 2300m);

    private static ApplyToJobHandler CreateApplyHandler(PostgreSqlContext context, long candidateId)
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(candidateId);
        currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = candidateId,
                Username = "candidato",
                Email = "candidato@test.local",
                Roles = ["Candidate"],
                Claims = []
            }
        });

        return new ApplyToJobHandler(
            new JobRepository(context),
            new JobApplicationRepository(context),
            currentUser.Object,
            new ApplyToJobCommandValidator(),
            NullLogger<ApplyToJobHandler>.Instance);
    }

    private static CancelJobApplicationHandler CreateCancelHandler(PostgreSqlContext context, long candidateId)
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(candidateId);
        currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = candidateId,
                Username = "candidato",
                Email = "candidato@test.local",
                Roles = ["Candidate"],
                Claims = []
            }
        });

        return new CancelJobApplicationHandler(
            new JobApplicationRepository(context),
            currentUser.Object,
            NullLogger<CancelJobApplicationHandler>.Instance);
    }

    [Fact]
    public async Task Handle_CandidaturaAnteriorCanceladaPeloCandidato_DevePermitirNovaCandidatura()
    {
        const long candidateId = 8201;
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 4001);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var firstId = await CreateApplyHandler(context, candidateId)
            .Handle(new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)), CancellationToken.None);

        await CreateCancelHandler(context, candidateId)
            .Handle(new CancelJobApplicationCommand(firstId), CancellationToken.None);

        var secondId = await CreateApplyHandler(context, candidateId)
            .Handle(new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)), CancellationToken.None);

        // Linha nova, e não reactivação da antiga: o histórico da desistência tem de sobreviver.
        secondId.Should().NotBe(firstId);

        var repository = new JobApplicationRepository(context);
        (await repository.GetByIdAsync(firstId, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        (await repository.GetByIdAsync(secondId, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Pending);
    }

    [Fact]
    public async Task Handle_CandidaturaActivaNaMesmaVaga_DeveContinuarARecusarDuplicata()
    {
        const long candidateId = 8202;
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 4002);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var handler = CreateApplyHandler(context, candidateId);
        await handler.Handle(new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)), CancellationToken.None);

        var act = async () => await handler.Handle(
            new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)),
            CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
    }

    // Poder recandidatar-se não abre a vaga encerrada: a desistência não recupera um processo que a
    // empresa já fechou.
    [Fact]
    public async Task Handle_VagaEncerradaAposCancelamento_DeveContinuarARecusar()
    {
        const long candidateId = 8203;
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var jobRepository = new JobRepository(context);
        var job = CreateJob(companyId: 4003);
        await jobRepository.CreateAsync(job, CancellationToken.None);

        var applicationId = await CreateApplyHandler(context, candidateId)
            .Handle(new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)), CancellationToken.None);

        await CreateCancelHandler(context, candidateId)
            .Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None);

        job.Close();
        await jobRepository.UpdateAsync(job, CancellationToken.None);

        var act = async () => await CreateApplyHandler(context, candidateId).Handle(
            new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)),
            CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
    }

    // O feed decide "Já candidatado" por esta consulta; se a cancelada continuasse a contar, o botão
    // de candidatura ficaria bloqueado numa recandidatura que a API aceita.
    [Fact]
    public async Task GetAppliedJobIds_AposCancelamento_NaoDeveMarcarAVagaComoCandidatada()
    {
        const long candidateId = 8204;
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 4004);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var applicationId = await CreateApplyHandler(context, candidateId)
            .Handle(new CreateCommand<ApplyToJobCommand>(new ApplyToJobCommand(job.Id)), CancellationToken.None);

        var repository = new JobApplicationRepository(context);
        (await repository.GetAppliedJobIdsAsync(candidateId, [job.Id], CancellationToken.None))
            .Should().Contain(job.Id);

        await CreateCancelHandler(context, candidateId)
            .Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None);

        (await repository.GetAppliedJobIdsAsync(candidateId, [job.Id], CancellationToken.None))
            .Should().BeEmpty();
    }
}
