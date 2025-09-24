using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmpregaNet.Infra.Persistence.Repositories;

/// <summary>
/// Implementa o padrão Unit of Work, encapsulando operações de persistência
/// de forma resiliente e atômica.
/// </summary>
/// <remarks>
/// Esta classe é responsável por:
/// <list type="bullet">
///  <item>Salvar todas as mudanças rastreadas pelo DbContext em uma única chamada (<c>SaveChangesAsync</c>).</item>
///  <item>Executar operações de forma segura dentro de uma transação de banco de dados, utilizando a estratégia de execução do ORM para garantir resiliência e retries em caso de falhas transitórias (<c>ExecuteInTransactionAsync</c>).</item>
/// </list>
///
/// O <c>UnityOfWork</c> atua como a única ponte entre o pipeline de negócio e a persistência,
/// garantindo que o ciclo de vida das transações seja gerenciado de forma segura e centralizada,
/// em alinhamento com os princípios de Inversão de Dependência e Separação de Responsabilidades.
/// </remarks>
public class UnityOfWork : IUnityOfWork
{
    private readonly PostgreSqlContext _context;

    public UnityOfWork(PostgreSqlContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            state: _context,
            operation: async (dbContext, context, token) =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(token);

                try
                {
                    var result = await operation();

                    await context.SaveChangesAsync(token);
                    await transaction.CommitAsync(token);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(token);
                    throw;
                }
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken
        );
    }
}