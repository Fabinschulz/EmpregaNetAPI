namespace EmpregaNet.Domain.Interfaces;

/// <summary>
/// Contrato base para agentes de IA no EmpregaNet (implementações em EmpregaNet.AI).
/// </summary>
public interface IAgent<in TData, TResponse>
    where TData : class
    where TResponse : class
{
    Task<TResponse> ExecuteAsync(TData data, CancellationToken cancellationToken = default);
}
