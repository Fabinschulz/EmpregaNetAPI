namespace Bff.Core.Interfaces;

/// <summary>
/// Interface para realizar chamadas a uma API SOAP.
/// </summary>
public interface ISoapApiClient
{
    /// <summary>
    /// Envia uma requisição SOAP para a API.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="envelope"></param>
    /// <returns>
    /// A task representa uma operação assíncrona que retorna o resultado da requisição SOAP.
    /// </returns>
    Task<T> SendRequestAsync<T>(string action, string envelope);
}