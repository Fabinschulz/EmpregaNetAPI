using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Extensions;

public class RateLimit
{
    public const string SectionName = "RateLimiting";
    public const string PolicyName = "FixedWindowPolicy";

    public int PermitLimit { get; set; } = 5;
    public int WindowInSeconds { get; set; } = 10;
    public int QueueLimit { get; set; } = 0;
}

public static class RateLimiterExtensions
{
    public static IServiceCollection SetupRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimit.SectionName).Get<RateLimit>() ?? new RateLimit();

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // Única política, particionada por IP: cada cliente tem o seu contador,
            // então um cliente abusivo não esgota o orçamento dos demais.
            rateLimiterOptions.AddPolicy(RateLimit.PolicyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.QueueLimit
                    }));

            rateLimiterOptions.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.WriteAsync(
                    "Atingido o limite de requisições. Tente novamente mais tarde.", cancellationToken);
                return new ValueTask();
            };
        });

        return services;
    }

    /// <summary>
    /// Chave de partição do rate limiter = IP do cliente.
    /// Nota: atrás de proxy reverso, configure ForwardedHeaders com KnownProxies para obter o IP real
    /// sem permitir spoofing via X-Forwarded-For.
    /// </summary>
    private static string GetClientPartitionKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
