using System.Net.Http.Json;
using System.Text.Json;
using Bff.Core.Interfaces;

internal sealed class RestApiClient : IRestApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RestApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TResponse?> PostAsync<TResponse>(string relativeUrl, object body, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(relativeUrl, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }
}