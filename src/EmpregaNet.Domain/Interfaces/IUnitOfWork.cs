namespace EmpregaNet.Domain.Interfaces;

/// <summary>
/// Define a unidade de trabalho para gerenciar transações de banco de dados.
/// </summary>
public interface IUnityOfWork
{
    /// <summary>
    /// Executa uma função de forma atômica e resiliente dentro de uma transação,
    /// usando a estratégia de execução configurada.
    /// </summary>
    /// <typeparam name="TResult">O tipo de retorno da função.</typeparam>
    /// <param name="operation">A função assíncrona a ser executada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O resultado da operação.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default);
}