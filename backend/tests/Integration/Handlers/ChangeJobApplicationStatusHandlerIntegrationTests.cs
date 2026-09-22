using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Application.Jobs.UseCase;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// O encerramento automático por preenchimento, de ponta a ponta contra repositórios reais: a
/// aprovação que fecha a última posição encerra a vaga e arrasta as candidaturas que ainda
/// estavam em aberto — o mesmo efeito do botão "Encerrar vaga" (ver
/// <see cref="CloseJobHandlerIntegrationTests"/>), só que disparado pela transição de status, não
/// por um clique do recrutador.
/// </summary>
/// <remarks>
/// Integração, e não unitário com mocks, porque o comportamento depende de duas escritas
/// coordenadas (candidatura e vaga) e de uma consulta subsequente (o arrasto) enxergando o que a
/// primeira escrita gravou — exactamente o que um repositório mockado não pode provar.
///
/// <para>
/// Limitação do provider InMemory: não reproduz o <c>SELECT … FOR UPDATE</c> nem a semântica de
/// transacção do PostgreSQL. O que fica provado aqui é o resultado da coordenação — contador,
/// estado da vaga e cascata — e não a serialização entre aprovações concorrentes.
/// </para>
/// </remarks>
[Collection("Integration")]
public sealed class ChangeJobApplicationStatusHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public ChangeJobApplicationStatusHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Job CreateJob(long companyId, int positions) => new(
        companyId: companyId,
        title: $"Auxiliar de Producao {Guid.NewGuid():N}",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.SemExperiencia,
        area: JobAreaEnum.Producao,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        positions: positions,
        salaryMin: 2200m);

    private static ChangeJobApplicationStatusCommandHandler CreateSut(
        PostgreSqlContext context,
        RecordingDomainEventQueue domainEvents)
    {
        var employerAccess = new Mock<IJobEmployerAccess>();
        employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var closureCascade = new JobClosureCascade(
            new JobApplicationRepository(context),
            domainEvents,
            NullLogger<JobClosureCascade>.Instance);

        return new ChangeJobApplicationStatusCommandHandler(
            new JobApplicationRepository(context),
            new JobRepository(context),
            employerAccess.Object,
            closureCascade,
            domainEvents,
            NullLogger<ChangeJobApplicationStatusCommandHandler>.Instance);
    }

    private static Task<JobApplicationViewModel> ExecuteAsync(
        ChangeJobApplicationStatusCommandHandler sut,
        long applicationId,
        ApplicationStatusEnum status)
        => sut.Handle(
            new UpdateCommand<ChangeJobApplicationStatusCommand, JobApplicationViewModel>(
                applicationId,
                new ChangeJobApplicationStatusCommand(status.ToString())),
            CancellationToken.None);

    /// <summary>
    /// O handler não grava por si: conta com o <c>SaveChangesAsync</c> do <c>UnityOfWork</c>, que
    /// aqui não corre — estes testes chamam o handler directamente, sem o pipeline.
    /// </summary>
    private static Task PersistAsync(PostgreSqlContext context) => context.SaveChangesAsync();

    private static async Task<long> SeedApplicationAsync(PostgreSqlContext context, long jobId, long userId)
    {
        var repository = new JobApplicationRepository(context);
        var application = new JobApplication(jobId, userId);
        await repository.CreateAsync(application, CancellationToken.None);
        return application.Id;
    }

    [Fact]
    public async Task Handle_AprovarAUltimaPosicao_DeveEncerrarAVagaEArrastarAsCandidaturasEmAberto()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 8101, positions: 1);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var aprovar = await SeedApplicationAsync(context, job.Id, 8201);
        var emAnalise = await SeedApplicationAsync(context, job.Id, 8202);
        var recebida = await SeedApplicationAsync(context, job.Id, 8203);

        var domainEvents = new RecordingDomainEventQueue();
        var result = await ExecuteAsync(CreateSut(context, domainEvents), aprovar, ApplicationStatusEnum.Approved);
        await PersistAsync(context);

        result.Status.Should().Be(ApplicationStatusEnum.Approved);

        var reloadedJob = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloadedJob.IsActive.Should().BeFalse();
        reloadedJob.FilledPositions.Should().Be(1);
        reloadedJob.AvailablePositions.Should().Be(0);
        reloadedJob.ClosureReason.Should().Be(JobClosureReasonEnum.Fulfilled);
        reloadedJob.ClosedAt.Should().NotBeNull();

        var applications = new JobApplicationRepository(context);
        (await applications.GetByIdAsync(aprovar, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Approved, "é a candidatura que preencheu a vaga");
        (await applications.GetByIdAsync(emAnalise, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Canceled, "arrastada pelo encerramento automático");
        (await applications.GetByIdAsync(recebida, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Canceled);

        var eventos = domainEvents.RecordedOf<JobApplicationStatusChanged>();
        eventos.Should().HaveCount(3, "a própria aprovação mais as duas arrastadas");

        eventos.Should().ContainSingle(e => e.JobApplicationId == aprovar
            && e.Reason == JobApplicationNotificationReason.StatusChanged
            && e.NewStatus == ApplicationStatusEnum.Approved);

        var arrastados = eventos.Where(e => e.JobApplicationId != aprovar).ToList();
        arrastados.Should().HaveCount(2);
        arrastados.Should().OnlyContain(e => e.Reason == JobApplicationNotificationReason.JobFilled);
        arrastados.Select(e => e.JobApplicationId).Should().BeEquivalentTo(new[] { emAnalise, recebida });
    }

    // O contrário do teste acima: sobrando posição, aprovar não encerra nada nem arrasta ninguém.
    [Fact]
    public async Task Handle_AprovarComPosicoesSobrando_NaoDeveEncerrarNemArrastar()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 8102, positions: 3);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var aprovar = await SeedApplicationAsync(context, job.Id, 8211);
        var outraEmAberto = await SeedApplicationAsync(context, job.Id, 8212);

        var domainEvents = new RecordingDomainEventQueue();
        await ExecuteAsync(CreateSut(context, domainEvents), aprovar, ApplicationStatusEnum.Approved);
        await PersistAsync(context);

        var reloadedJob = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloadedJob.IsActive.Should().BeTrue();
        reloadedJob.FilledPositions.Should().Be(1);
        reloadedJob.AvailablePositions.Should().Be(2);

        (await new JobApplicationRepository(context).GetByIdAsync(outraEmAberto, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Pending, "sem cascata, nada arrasta quem continua em aberto");

        domainEvents.RecordedOf<JobApplicationStatusChanged>().Should().ContainSingle()
            .Which.Reason.Should().Be(JobApplicationNotificationReason.StatusChanged);
    }

    // Concluir o processo de quem já foi aprovado não pode devolver a posição — senão a vaga
    // reabriria sozinha só porque alguém terminou o próprio funil.
    [Fact]
    public async Task Handle_AprovadoParaConcluido_NaoDeveLibertarAPosicaoNemAlterarAVaga()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 8103, positions: 2);
        job.FillPosition();
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var repository = new JobApplicationRepository(context);
        var application = new JobApplication(job.Id, 8221);
        application.ChangeStatus(ApplicationStatusEnum.Approved);
        await repository.CreateAsync(application, CancellationToken.None);
        context.ChangeTracker.Clear();

        var domainEvents = new RecordingDomainEventQueue();
        await ExecuteAsync(CreateSut(context, domainEvents), application.Id, ApplicationStatusEnum.Finished);
        await PersistAsync(context);

        var reloadedJob = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloadedJob.FilledPositions.Should().Be(1, "concluir o processo não é o mesmo que perder a posição");
        reloadedJob.IsActive.Should().BeTrue();
    }

    // Reprovar quem estava aprovado devolve a posição à vaga — é o caminho inverso da aprovação.
    [Fact]
    public async Task Handle_AprovadoParaReprovado_DeveLibertarAPosicao()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 8104, positions: 2);
        job.FillPosition();
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var repository = new JobApplicationRepository(context);
        var application = new JobApplication(job.Id, 8231);
        application.ChangeStatus(ApplicationStatusEnum.Approved);
        await repository.CreateAsync(application, CancellationToken.None);
        context.ChangeTracker.Clear();

        var domainEvents = new RecordingDomainEventQueue();
        await ExecuteAsync(CreateSut(context, domainEvents), application.Id, ApplicationStatusEnum.Rejected);
        await PersistAsync(context);

        var reloadedJob = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloadedJob.FilledPositions.Should().Be(0);
        reloadedJob.AvailablePositions.Should().Be(2);
        reloadedJob.IsActive.Should().BeTrue();
    }

    // Janela de corrida simulada: a vaga já fechou (por outra aprovação) antes deste pedido chegar
    // ao handler. A recusa tem de ser erro de negócio, e nada pode ser persistido.
    [Fact]
    public async Task Handle_AprovarNumaVagaJaFechada_DeveDevolverErroDeNegocioSemPersistirNada()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 8105, positions: 1);
        job.FillPosition();
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var atrasada = await SeedApplicationAsync(context, job.Id, 8241);
        context.ChangeTracker.Clear();

        var domainEvents = new RecordingDomainEventQueue();
        var act = async () => await ExecuteAsync(CreateSut(context, domainEvents), atrasada, ApplicationStatusEnum.Approved);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);

        domainEvents.Recorded.Should().BeEmpty();

        // A candidatura foi mutada em memória antes da recusa (ChangeStatus corre antes do guard de
        // posições); só o Clear() garante que a leitura seguinte vem do que foi de facto gravado, e
        // não da instância suja ainda presa ao change tracker.
        context.ChangeTracker.Clear();
        var reloadedApplication = await context.JobApplications.AsNoTracking().SingleAsync(a => a.Id == atrasada);
        reloadedApplication.Status.Should().Be(ApplicationStatusEnum.Pending, "a recusa não pode ter mutado a candidatura");
    }
}
