using Bff.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Bff.Infrastructure;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddHttpApiClient(
        this IServiceCollection services,
        string baseAddress)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retry =>
                TimeSpan.FromSeconds(Math.Pow(2, retry)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IRestApiClient, RestApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Accept
                .Add(new("application/json"));
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }
}