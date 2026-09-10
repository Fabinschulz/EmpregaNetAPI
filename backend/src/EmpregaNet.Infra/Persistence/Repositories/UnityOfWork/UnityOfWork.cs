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

    /// <summary>
    /// Executa <paramref name="operation"/> em uma transação com retry do provider para tratar falhas transitórias.
    /// </summary>
    /// <remarks>
    /// A estratégia de execução do EF reexecuta a operação inteira quando a falha é classificada como transitória
    /// (<c>EnableRetryOnFailure(maxRetryCount: 2)</c> em <c>DatabaseConfig</c>).
    ///
    /// <para>
    /// A limpeza do <c>ChangeTracker</c> só ocorre na segunda tentativa. Isso evita reutilizar entidades
    /// rastreadas com estado sujo da tentativa anterior, o que pode causar guardas falsas ou updates silenciosos
    /// sem persistência real.
    /// </para>
    ///
    /// <para>
    /// Esta classe limpa apenas o estado do EF. Efeitos externos, como cache, fila de eventos e e-mails,
    /// permanecem sob responsabilidade dos handlers, que devem evitar ações que não possam ser desfeitas por um rollback.
    /// </para>
    ///
    /// <para>
    /// O parâmetro <c>verifySucceeded</c> continua <c>null</c> por decisão de projeto. Se o commit for confirmado
    /// e a confirmação for perdida, a próxima tentativa verá o estado já persistido e a guarda pode disparar com
    /// a mensagem correta, mesmo que a operação tenha sido concluída.
    /// </para>
    /// </remarks>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        var attempt = 0;

        return await strategy.ExecuteAsync(
            state: _context,
            operation: async (dbContext, context, token) =>
            {
                if (attempt++ > 0)
                {
                    context.ChangeTracker.Clear();
                }

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
