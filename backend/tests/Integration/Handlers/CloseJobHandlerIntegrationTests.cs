using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Application.Jobs.Commands;
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
/// Encerrar a vaga arrasta as candidaturas em aberto e notifica cada candidato (V3, N6, CA-06).
/// </summary>
/// <remarks>
/// Integração porque a regra depende de uma consulta (quais candidaturas estão em aberto) e de
/// escrita em N linhas: com repositório mockado, o teste afirmaria apenas o que ele próprio mandou
/// devolver.
///
/// <para>
/// Limitação do provider InMemory: não reproduz constraints, transacções nem semântica do PostgreSQL.
/// Em particular, <b>não</b> prova a atomicidade que o handler assume — o que se prova aqui é quais
/// candidaturas mudam de estado, quais ficam intocadas e quantos eventos são enfileirados.
/// </para>
/// </remarks>
[Collection("Integration")]
public sealed class CloseJobHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public CloseJobHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Job CreateJob(long companyId) => new(
        companyId: companyId,
        title: $"Auxiliar de Logistica {Guid.NewGuid():N}",
        description: "Conferencia de cargas.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.AteUmAno,
        area: JobAreaEnum.Logistica,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        salaryMin: 2200m);

    private static CloseJobHandler CreateSut(
        PostgreSqlContext context,
        RecordingDomainEventQueue domainEvents,
        string role = "Recruiter")
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(5000);
        currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = 5000,
                Username = "recrutador",
                Email = "recrutador@test.local",
                Roles = [role],
                Claims = []
            }
        });

        var employerAccess = new Mock<IJobEmployerAccess>();
        employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new CloseJobHandler(
            new JobRepository(context),
            new JobApplicationRepository(context),
            domainEvents,
            new CloseJobCommandValidator(),
            NullLogger<CloseJobHandler>.Instance,
            currentUser.Object,
            employerAccess.Object);
    }

    /// <summary>
    /// O handler <b>não</b> grava: altera entidades rastreadas e conta com o <c>SaveChangesAsync</c>
    /// do <c>UnityOfWork</c>, no fim da transacção. Como estes testes chamam o handler directamente,
    /// sem o pipeline, a gravação é feita aqui — senão as asserções liam o <i>identity map</i> em
    /// memória e passariam mesmo que nada fosse escrito.
    /// </summary>
    private static Task PersistAsync(PostgreSqlContext context) => context.SaveChangesAsync();

    private static async Task<long> SeedApplicationAsync(
        PostgreSqlContext context,
        long jobId,
        long userId,
        ApplicationStatusEnum status)
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

        return application.Id;
    }

    [Fact]
    public async Task Handle_VagaComCandidaturasEmAberto_DeveCancelarTodasEEnfileirarUmEventoPorCandidatura()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 6001);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var recebida = await SeedApplicationAsync(context, job.Id, 6101, ApplicationStatusEnum.Pending);
        var emAnalise = await SeedApplicationAsync(context, job.Id, 6102, ApplicationStatusEnum.Processing);
        var outraEmAnalise = await SeedApplicationAsync(context, job.Id, 6103, ApplicationStatusEnum.Processing);

        var domainEvents = new RecordingDomainEventQueue();
        var result = await CreateSut(context, domainEvents).Handle(new CloseJobCommand(job.Id), CancellationToken.None);
        await PersistAsync(context);

        result.JobId.Should().Be(job.Id);
        result.AffectedApplications.Should().Be(3);
        result.ClosedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));

        var repository = new JobApplicationRepository(context);
        foreach (var id in new[] { recebida, emAnalise, outraEmAnalise })
        {
            (await repository.GetByIdAsync(id, CancellationToken.None))!.Status
                .Should().Be(ApplicationStatusEnum.Canceled, "é o ato da empresa, não do candidato");
        }

        var eventos = domainEvents.RecordedOf<JobApplicationStatusChanged>();
        eventos.Should().HaveCount(3);
        eventos.Should().OnlyContain(e => e.Reason == JobApplicationNotificationReason.JobClosed);
        eventos.Select(e => e.JobApplicationId).Should().BeEquivalentTo(new[] { recebida, emAnalise, outraEmAnalise });
    }

    /// <summary>
    /// O histórico de quem já teve desfecho não se reescreve por a vaga ter fechado — e quem já
    /// desistiu não recebe um aviso de encerramento.
    /// </summary>
    [Fact]
    public async Task Handle_CandidaturasComDesfecho_NaoDevemSerTocadasNemNotificadas()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 6002);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var aprovada = await SeedApplicationAsync(context, job.Id, 6201, ApplicationStatusEnum.Approved);
        var reprovada = await SeedApplicationAsync(context, job.Id, 6202, ApplicationStatusEnum.Rejected);
        var concluida = await SeedApplicationAsync(context, job.Id, 6203, ApplicationStatusEnum.Finished);
        var desistiu = await SeedApplicationAsync(context, job.Id, 6204, ApplicationStatusEnum.CanceledByCandidate);
        var emAberto = await SeedApplicationAsync(context, job.Id, 6205, ApplicationStatusEnum.Processing);

        var domainEvents = new RecordingDomainEventQueue();
        var result = await CreateSut(context, domainEvents).Handle(new CloseJobCommand(job.Id), CancellationToken.None);
        await PersistAsync(context);

        result.AffectedApplications.Should().Be(1);

        var repository = new JobApplicationRepository(context);
        (await repository.GetByIdAsync(aprovada, CancellationToken.None))!.Status.Should().Be(ApplicationStatusEnum.Approved);
        (await repository.GetByIdAsync(reprovada, CancellationToken.None))!.Status.Should().Be(ApplicationStatusEnum.Rejected);
        (await repository.GetByIdAsync(concluida, CancellationToken.None))!.Status.Should().Be(ApplicationStatusEnum.Finished);
        (await repository.GetByIdAsync(desistiu, CancellationToken.None))!.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        (await repository.GetByIdAsync(emAberto, CancellationToken.None))!.Status.Should().Be(ApplicationStatusEnum.Canceled);

        var eventos = domainEvents.RecordedOf<JobApplicationStatusChanged>();
        eventos.Should().ContainSingle().Which.JobApplicationId.Should().Be(emAberto);
    }

    [Fact]
    public async Task Handle_VagaSemCandidaturas_DeveEncerrarSemEventos()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 6003);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var domainEvents = new RecordingDomainEventQueue();
        var result = await CreateSut(context, domainEvents).Handle(new CloseJobCommand(job.Id), CancellationToken.None);
        await PersistAsync(context);

        result.AffectedApplications.Should().Be(0);
        domainEvents.Recorded.Should().BeEmpty();
        (await new JobRepository(context).GetByIdAsync(job.Id, CancellationToken.None))!.IsActive.Should().BeFalse();
    }

    // Candidaturas de outra vaga não são arrastadas: o filtro é por vaga, não global.
    [Fact]
    public async Task Handle_CandidaturasDeOutraVaga_NaoDevemSerAfectadas()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var jobRepository = new JobRepository(context);
        var encerrada = CreateJob(companyId: 6004);
        var outra = CreateJob(companyId: 6004);
        await jobRepository.CreateAsync(encerrada, CancellationToken.None);
        await jobRepository.CreateAsync(outra, CancellationToken.None);

        await SeedApplicationAsync(context, encerrada.Id, 6301, ApplicationStatusEnum.Processing);
        var intocada = await SeedApplicationAsync(context, outra.Id, 6302, ApplicationStatusEnum.Processing);

        var domainEvents = new RecordingDomainEventQueue();
        var result = await CreateSut(context, domainEvents).Handle(new CloseJobCommand(encerrada.Id), CancellationToken.None);
        await PersistAsync(context);

        result.AffectedApplications.Should().Be(1);
        (await new JobApplicationRepository(context).GetByIdAsync(intocada, CancellationToken.None))!.Status
            .Should().Be(ApplicationStatusEnum.Processing);
    }

    [Fact]
    public async Task Handle_VagaJaEncerrada_DeveRecusarSemEnfileirarEvento()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var jobRepository = new JobRepository(context);
        var job = CreateJob(companyId: 6005);
        await jobRepository.CreateAsync(job, CancellationToken.None);
        job.Close();
        await jobRepository.UpdateAsync(job, CancellationToken.None);

        await SeedApplicationAsync(context, job.Id, 6401, ApplicationStatusEnum.Processing);

        var domainEvents = new RecordingDomainEventQueue();
        var act = async () => await CreateSut(context, domainEvents)
            .Handle(new CloseJobCommand(job.Id), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        domainEvents.Recorded.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_VagaInexistente_DeveRecusar()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var act = async () => await CreateSut(context, new RecordingDomainEventQueue())
            .Handle(new CloseJobCommand(987321), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
    }

    // Defesa em profundidade: encerrar vaga é ato do recrutamento, verificado na Application e não só
    // pelo [Authorize] do controller.
    [Fact]
    public async Task Handle_PerfilSemRecrutamento_DeveRecusar()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 6006);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        var act = async () => await CreateSut(context, new RecordingDomainEventQueue(), role: "Candidate")
            .Handle(new CloseJobCommand(job.Id), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }
}
