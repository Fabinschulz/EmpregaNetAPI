using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Leitura da vaga com a linha bloqueada, usada pela transição de status da candidatura.
/// </summary>
/// <remarks>
/// <b>Limitação:</b> o provider InMemory não executa SQL bruto, portanto o que estes testes cobrem é
/// o ramo de degradação — a entidade volta rastreada e as alterações chegam ao banco. O
/// <c>SELECT … FOR UPDATE</c> em si, e a serialização que ele garante, só se verificam contra o
/// PostgreSQL; aqui prova-se que o método é utilizável no lugar de <c>GetByIdAsync</c>.
/// </remarks>
[Collection("Integration")]
public sealed class JobRepositoryForUpdateIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public JobRepositoryForUpdateIntegrationTests(InMemoryIdentityFixture fixture)
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
        positions: positions);

    [Fact]
    public async Task GetByIdForUpdateAsync_VagaInexistente_DeveDevolverNulo()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var found = await new JobRepository(context).GetByIdForUpdateAsync(long.MaxValue, CancellationToken.None);

        found.Should().BeNull();
    }

    // Rastreada, e não AsNoTracking: quem chama vai preencher uma posição e gravar.
    [Fact]
    public async Task GetByIdForUpdateAsync_DevePermitirPreencherPosicaoEPersistir()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        var repository = new JobRepository(context);

        var job = CreateJob(companyId: 7101, positions: 2);
        await repository.CreateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        var tracked = await repository.GetByIdForUpdateAsync(job.Id, CancellationToken.None);
        tracked.Should().NotBeNull();

        tracked!.FillPosition().Should().BeFalse("ainda sobra uma posição");
        await repository.UpdateAsync(tracked, CancellationToken.None);
        context.ChangeTracker.Clear();

        var reloaded = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloaded.FilledPositions.Should().Be(1);
        reloaded.AvailablePositions.Should().Be(1);
        reloaded.IsActive.Should().BeTrue();
    }

    // Encerrar por preenchimento tem de sobreviver ao round-trip: é o motivo que a UI lê para
    // distinguir "a empresa encerrou" de "as vagas encheram".
    [Fact]
    public async Task EncerramentoPorPreenchimento_DevePersistirMotivoEData()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        var repository = new JobRepository(context);

        var job = CreateJob(companyId: 7102, positions: 1);
        await repository.CreateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        var tracked = await repository.GetByIdForUpdateAsync(job.Id, CancellationToken.None);
        tracked!.FillPosition().Should().BeTrue("era a última posição");
        await repository.UpdateAsync(tracked, CancellationToken.None);
        context.ChangeTracker.Clear();

        var reloaded = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloaded.IsActive.Should().BeFalse();
        reloaded.ClosureReason.Should().Be(JobClosureReasonEnum.Fulfilled);
        reloaded.ClosedAt.Should().NotBeNull();
        reloaded.Status.Should().Be(JobStatusEnum.ClosedByFulfillment);
    }
}
