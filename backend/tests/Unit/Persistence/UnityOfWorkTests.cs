using System.Reflection;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmpregaNet.Tests.Unit.Persistence;

/// <summary>
/// O <c>UnityOfWork</c> repõe o <c>ChangeTracker</c> <b>a partir da segunda tentativa</b> da estratégia
/// de execução, e não antes da primeira.
/// </summary>
/// <remarks>
/// <b>Porque a condição é o comportamento sob teste.</b> A reexecução por retry transitório corre sobre
/// o <b>mesmo</b> <c>DbContext</c>: sem repor o tracker, a segunda tentativa reencontra as entidades com
/// a mutação da tentativa revertida — daí erro de negócio falso onde há guarda, e <c>UPDATE</c> nunca
/// emitido onde não há. Limpar <b>antes</b> da primeira seria a correcção preguiçosa e teria outro
/// custo: destacaria entidades rastreadas mais cedo na requisição por código alheio à transacção
/// (Identity, <c>UserManager</c> em <c>JobEmployerAccess</c>).
///
/// <para>
/// <b>O que estes testes não cobrem.</b> O provider real é o PostgreSQL com
/// <c>EnableRetryOnFailure</c>; aqui a base é InMemory, onde a transacção é um no-op — portanto o
/// <b>rollback</b> não é reproduzido e a escrita perdida em silêncio não é observável. O que se observa
/// é o que interessa fixar: <b>quando</b> o tracker é reposto. A estratégia com retry é injectada pelo
/// ponto de extensão do próprio EF (<see cref="IExecutionStrategyFactory"/> via
/// <c>ReplaceService</c>), sem nenhuma abstracção nova em produção. Cobertura do comportamento
/// transaccional real exige base real (Testcontainers), que este projecto não tem.
/// </para>
/// </remarks>
public sealed class UnityOfWorkTests
{
    /// <summary>Falha que a estratégia de teste classifica como transitória.</summary>
    private sealed class TransientTestException : Exception
    {
        public TransientTestException() : base("Falha transitória simulada.") { }
    }

    /// <summary>
    /// Estratégia com uma repetição, para exercitar a segunda tentativa. Só repete a excepção
    /// marcadora: qualquer outra falha continua a propagar de imediato, como em produção.
    /// </summary>
    private sealed class RetryOnceExecutionStrategy : ExecutionStrategy
    {
        public RetryOnceExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, maxRetryCount: 1, maxRetryDelay: TimeSpan.Zero)
        {
        }

        protected override bool ShouldRetryOn(Exception exception) => exception is TransientTestException;
    }

    private sealed class RetryOnceExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly ExecutionStrategyDependencies _dependencies;

        public RetryOnceExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
            => _dependencies = dependencies;

        public IExecutionStrategy Create() => new RetryOnceExecutionStrategy(_dependencies);
    }

    private static PostgreSqlContext CreateContext(bool withRetry)
    {
        var builder = new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseInMemoryDatabase($"uow_{Guid.NewGuid():N}")
            // O InMemory não tem transacções; sem isto, BeginTransactionAsync lança.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        if (withRetry)
        {
            builder.ReplaceService<IExecutionStrategyFactory, RetryOnceExecutionStrategyFactory>();
        }

        return new PostgreSqlContext(builder.Options);
    }

    /// <summary>Simula o que código anterior à transacção deixou rastreado (estado <c>Unchanged</c>).</summary>
    private static JobApplication TrackExistingApplication(PostgreSqlContext context, long id)
    {
        var application = new JobApplication(jobId: 10, userId: 20);
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(application, id);

        context.Attach(application);
        return application;
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_PrimeiraTentativa_NaoDeveLimparOChangeTracker()
    {
        using var context = CreateContext(withRetry: false);
        var application = TrackExistingApplication(context, id: 1);
        var sut = new UnityOfWork(context);

        var trackedDuringOperation = -1;
        var result = await sut.ExecuteInTransactionAsync(() =>
        {
            trackedDuringOperation = context.ChangeTracker.Entries().Count();
            return Task.FromResult("ok");
        });

        result.Should().Be("ok");
        trackedDuringOperation.Should().Be(
            1,
            "limpar antes da primeira tentativa destacaria entidades de que a transacção não é dona");
        context.Entry(application).State.Should().Be(
            EntityState.Unchanged,
            "a entidade rastreada antes da transacção continua rastreada depois dela");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_SegundaTentativa_DeveLimparOChangeTracker()
    {
        using var context = CreateContext(withRetry: true);
        TrackExistingApplication(context, id: 1);
        var sut = new UnityOfWork(context);

        var trackedPerAttempt = new List<int>();
        var result = await sut.ExecuteInTransactionAsync(() =>
        {
            trackedPerAttempt.Add(context.ChangeTracker.Entries().Count());

            if (trackedPerAttempt.Count == 1)
            {
                throw new TransientTestException();
            }

            return Task.FromResult("ok");
        });

        result.Should().Be("ok");
        trackedPerAttempt.Should().Equal(
            [1, 0],
            "a primeira tentativa vê o que já estava rastreado; a segunda começa com o tracker reposto");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_FalhaNaoTransitoria_DevePropagarSemRepetir()
    {
        using var context = CreateContext(withRetry: true);
        var sut = new UnityOfWork(context);

        var attempts = 0;
        var act = async () => await sut.ExecuteInTransactionAsync<string>(() =>
        {
            attempts++;
            throw new InvalidOperationException("erro de negócio");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(1, "erro de negócio não é falha transitória e não se repete");
    }
}
