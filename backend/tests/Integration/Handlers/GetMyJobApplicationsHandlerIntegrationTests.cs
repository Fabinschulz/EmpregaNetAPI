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
/// Filtro por status na lista do candidato. O caso que interessa é o <c>Pending</c>: a opção
/// "Recebida" já existia na tela e nunca devolvia nada, porque nenhuma candidatura chegava a existir
/// nesse estado.
/// </summary>
/// <remarks>
/// Integração porque o filtro está na consulta do repositório, não no handler.
/// Limitação do provider InMemory: não reproduz constraints nem semântica do PostgreSQL.
/// </remarks>
[Collection("Integration")]
public sealed class GetMyJobApplicationsHandlerIntegrationTests
{
    private const long CandidateId = 7301;
    private const long OtherCandidateId = 7302;

    private readonly InMemoryIdentityFixture _fixture;

    public GetMyJobApplicationsHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static GetMyJobApplicationsHandler CreateSut(PostgreSqlContext context, long userId)
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        return new GetMyJobApplicationsHandler(
            new JobApplicationRepository(context),
            currentUser.Object,
            NullLogger<GetMyJobApplicationsHandler>.Instance);
    }

    private static async Task SeedAsync(PostgreSqlContext context, long userId, long jobId, ApplicationStatusEnum status)
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
    public async Task Handle_FiltroRecebida_DeveDevolverAsCandidaturasEmPending()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        await SeedAsync(context, CandidateId, jobId: 7311, ApplicationStatusEnum.Pending);
        await SeedAsync(context, CandidateId, jobId: 7312, ApplicationStatusEnum.Processing);
        await SeedAsync(context, CandidateId, jobId: 7313, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, OtherCandidateId, jobId: 7314, ApplicationStatusEnum.Pending);

        var result = await CreateSut(context, CandidateId)
            .Handle(new GetMyJobApplicationsQuery(1, 100, "Pending", null), CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data[0].JobId.Should().Be(7311);
        result.Data[0].Status.Should().Be(ApplicationStatusEnum.Pending);
    }

    // A cancelada continua na lista do candidato: o histórico é dele, e é por ele que a tela mostra
    // "Cancelada por você" em vez de a candidatura simplesmente desaparecer.
    [Fact]
    public async Task Handle_FiltroCanceladaPeloCandidato_DeveDevolverAsQueEleCancelou()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        await SeedAsync(context, CandidateId, jobId: 7321, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, CandidateId, jobId: 7322, ApplicationStatusEnum.Processing);

        var result = await CreateSut(context, CandidateId)
            .Handle(new GetMyJobApplicationsQuery(1, 100, "CanceledByCandidate", null), CancellationToken.None);

        result.Data.Should().OnlyContain(a => a.Status == ApplicationStatusEnum.CanceledByCandidate);
        result.Data.Should().Contain(a => a.JobId == 7321);
    }

    [Fact]
    public async Task Handle_SemFiltro_DeveDevolverTambemAsCanceladasPeloCandidato()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        await SeedAsync(context, CandidateId, jobId: 7331, ApplicationStatusEnum.CanceledByCandidate);

        var result = await CreateSut(context, CandidateId)
            .Handle(new GetMyJobApplicationsQuery(1, 100, null, null), CancellationToken.None);

        result.Data.Should().Contain(a => a.JobId == 7331 && a.Status == ApplicationStatusEnum.CanceledByCandidate);
    }
}
