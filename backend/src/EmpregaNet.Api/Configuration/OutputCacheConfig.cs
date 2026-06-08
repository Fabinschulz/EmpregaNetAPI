using EmpregaNet.Api.Configuration.OutputCache;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Cache;

namespace EmpregaNet.Api.Configuration;

// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-10.0
public static class OutputCacheConfig
{
    public static IServiceCollection SetupOutputCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutputCacheOptions>(configuration.GetSection(OutputCacheOptions.SectionName));

        var redis = RedisOptions.Resolve(configuration);
        if (redis.IsActive)
        {
            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = redis.ConnectionString;
                options.InstanceName = redis.InstanceName;
            });
        }

        var outputCacheSection = configuration.GetSection(OutputCacheOptions.SectionName);
        var cacheOptions = outputCacheSection.Get<OutputCacheOptions>() ?? new OutputCacheOptions();

        var sizeLimitMb = cacheOptions.SizeLimitMegabytes > 0 ? cacheOptions.SizeLimitMegabytes : 100;
        var maxBodyMb = cacheOptions.MaximumBodySizeMegabytes > 0 ? cacheOptions.MaximumBodySizeMegabytes : 100;
        var expirationMinutes = cacheOptions.DefaultExpirationMinutes > 0 ? cacheOptions.DefaultExpirationMinutes : 90;

        services.AddOutputCache(options =>
        {
            options.SizeLimit = sizeLimitMb * 1024L * 1024L;
            options.MaximumBodySize = maxBodyMb * 1024L * 1024L;
            options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(expirationMinutes);

            // OBS: nada é cacheado até o endpoint declarar [OutputCache(PolicyName = ...)].
            options.AddBasePolicy(builder => builder.NoCache());

            options.AddPolicy(OutputCachePolicies.PublicCatalog,
                builder => builder.AddPolicy<PublicCatalogOutputCachePolicy>());
            options.AddPolicy(OutputCachePolicies.EntityRead,
                builder => builder.AddPolicy<EntityReadOutputCachePolicy>());
            options.AddPolicy(OutputCachePolicies.AuthenticatedRead,
                builder => builder.AddPolicy<AuthenticatedReadOutputCachePolicy>());
            options.AddPolicy(OutputCachePolicies.UserProfileRead,
                builder => builder.AddPolicy<UserProfileReadOutputCachePolicy>());
        });

        services.AddSingleton<IOutputCacheManager, OutputCacheManager>();

        return services;
    }
}
