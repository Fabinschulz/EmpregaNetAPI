namespace Bff.Core.Interfaces;

/// <summary>
/// Interface para um cliente REST genérico.
/// </summary>
public interface IRestApiClient
{
    Task<TResponse?> PostAsync<TResponse>(string relativeUrl, object body, CancellationToken cancellationToken = default);
}