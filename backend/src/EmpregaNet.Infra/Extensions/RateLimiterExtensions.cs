using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
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
            rateLimiterOptions.AddFixedWindowLimiter(policyName: RateLimit.PolicyName, fixedWindowOptions =>
            {
                fixedWindowOptions.PermitLimit = options.PermitLimit;
                fixedWindowOptions.Window = TimeSpan.FromSeconds(options.WindowInSeconds);
                fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                fixedWindowOptions.QueueLimit = options.QueueLimit;
            });

            rateLimiterOptions.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.WriteAsync("Atingido o limite de requisições. Tente novamente mais tarde.", cancellationToken);
                return new ValueTask();
            };
        });

        return services;
    }
}