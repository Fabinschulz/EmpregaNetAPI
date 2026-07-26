using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Configuração do rate limit (seção "RateLimiting" do appsettings).
///
/// O algoritmo é <b>token bucket</b>: o balde começa cheio com <see cref="BurstCapacity"/> fichas
/// e repõe <see cref="SustainedPerPeriod"/> a cada <see cref="ReplenishmentPeriodInSeconds"/>.
/// Isso absorve o pico natural de uma tela que dispara várias chamadas em paralelo ao carregar,
/// enquanto continua a limitar a <i>taxa sustentada</i> que é o que caracteriza abuso.
/// Uma janela fixa não faz essa distinção: trata o pico legítimo e o abuso da mesma forma.
/// </summary>
public sealed class RateLimit
{
    public const string SectionName = "RateLimiting";
    public const string PolicyName = "GlobalPolicy";

    /// <summary>Desliga o limitador sem recompilar - válvula de escape em incidente.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Capacidade do balde: pico de requisições absorvido de uma vez.</summary>
    public int BurstCapacity { get; set; } = 120;

    /// <summary>Fichas repostas por período - define a taxa sustentada permitida.</summary>
    public int SustainedPerPeriod { get; set; } = 60;

    public int ReplenishmentPeriodInSeconds { get; set; } = 10;

    public int QueueLimit { get; set; } = 0;

    /// <summary>
    /// IPs isentos do limite. Necessário para origens confiáveis que concentram tráfego de
    /// muitos utilizadores num único IP - sobretudo o servidor Next.js, que busca dados no
    /// servidor (SSR) e, sem isenção, esgotaria sozinho o balde do seu próprio IP.
    /// </summary>
    public string[] BypassIps { get; set; } = [];
}

public static class RateLimiterExtensions
{
    public static IServiceCollection SetupRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimit.SectionName).Get<RateLimit>() ?? new RateLimit();

        var bypassIps = ParseBypassIps(options.BypassIps);

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddPolicy(RateLimit.PolicyName, httpContext =>
            {
                if (!options.Enabled || IsBypassed(httpContext, bypassIps))
                {
                    return RateLimitPartition.GetNoLimiter(PartitionKeys.Bypass);
                }

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = options.BurstCapacity,
                        TokensPerPeriod = options.SustainedPerPeriod,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(options.ReplenishmentPeriodInSeconds),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.QueueLimit
                    });
            });

            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var response = httpContext.Response;

                // Se o cliente já começou a receber a resposta não há como reescrevê-la.
                if (response.HasStarted)
                {
                    return;
                }

                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                    : options.ReplenishmentPeriodInSeconds;

                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                response.ContentType = "application/json";

                var error = new DomainError
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Code = DomainErrorEnum.TOO_MANY_REQUESTS,
                    Message = $"Muitas requisições em pouco tempo. Tente novamente em {retryAfterSeconds} segundo(s).",
                    Details = new { retryAfterSeconds },
                    CorrelationId = httpContext.Items["Correlation-ID"]?.ToString() ?? httpContext.TraceIdentifier
                };
                await response.WriteAsJsonAsync(error, cancellationToken);
            };
        });

        return services;
    }

    private static class PartitionKeys
    {
        public const string Bypass = "bypass";
        public const string Unknown = "ip:unknown";
    }

    private static string GetPartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue("userId")
                     ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        var ip = NormalizeIp(httpContext.Connection.RemoteIpAddress);
        return ip is null ? PartitionKeys.Unknown : $"ip:{ip}";
    }

    private static bool IsBypassed(HttpContext httpContext, IReadOnlySet<IPAddress> bypassIps)
    {
        if (bypassIps.Count == 0)
        {
            return false;
        }

        var ip = NormalizeIp(httpContext.Connection.RemoteIpAddress);
        return ip is not null && bypassIps.Contains(ip);
    }

    private static HashSet<IPAddress> ParseBypassIps(IEnumerable<string> raw)
    {
        var parsed = new HashSet<IPAddress>();

        foreach (var candidate in raw)
        {
            if (IPAddress.TryParse(candidate?.Trim(), out var ip) && NormalizeIp(ip) is { } normalized)
            {
                parsed.Add(normalized);
            }
        }

        return parsed;
    }

    /// <summary>Normaliza IPv4 mapeado em IPv6 (::ffff:127.0.0.1) para comparação estável.</summary>
    private static IPAddress? NormalizeIp(IPAddress? ip) =>
        ip is null ? null : ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
}
