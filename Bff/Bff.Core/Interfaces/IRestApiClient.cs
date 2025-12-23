using Refit;

namespace Bff.Core.Interfaces;

/// <summary>
/// Interface para um cliente REST genérico usando Refit.
/// </summary>
public interface IRestApiClient
{
    [Get("/{path}")]
    Task<IApiResponse<T>> GetAsync<T>(string path, [Header("Authorization")] string token = null!);

    [Post("/{path}")]
    Task<IApiResponse<T>> PostAsync<T>(string path, [Body] object data, [Header("Authorization")] string token = null!);
}