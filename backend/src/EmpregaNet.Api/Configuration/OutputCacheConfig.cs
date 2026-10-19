using EmpregaNet.Api.Configuration.OutputCache;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Infra.Cache;
using EmpregaNet.Infra.Persistence.Database;

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
            options.AddPolicy(OutputCachePolicies.DashboardRead,
                builder => builder.AddPolicy<DashboardReadOutputCachePolicy>());
        });

        services.AddSingleton<IOutputCacheManager, OutputCacheManager>();

        return services;
    }

    public const int MaxSafeExpirationMinutesWithoutSharedStore = 5;

    /// <summary>
    /// Sem Redis, a invalidação por tag só alcança a réplica que atendeu a escrita; as outras servem o catálogo
    /// público antigo até a validade acabar. Devolve o aviso, ou <c>null</c> quando não há o que avisar.
    /// </summary>
    public static string? StalePublicCatalogWarning(
        OutputCacheOptions cache, DatabaseOptions database, bool redisActive, bool isDevelopment)
    {
        if (isDevelopment || redisActive || database.ExpectedMaxReplicas <= 1
            || cache.DefaultExpirationMinutes <= MaxSafeExpirationMinutesWithoutSharedStore)
        {
            return null;
        }

        return $"OutputCache:DefaultExpirationMinutes = {cache.DefaultExpirationMinutes} com {database.ExpectedMaxReplicas} " +
               "réplicas previstas e sem Redis: a invalidação do cache é local a cada réplica, então o catálogo público " +
               $"pode ficar até {cache.DefaultExpirationMinutes} minuto(s) desatualizado nas outras réplicas. " +
               $"Use no máximo {MaxSafeExpirationMinutesWithoutSharedStore} minutos ou ative o Redis.";
    }

    public static void WarnIfPublicCacheCanBeStale(WebApplication app)
    {
        var cache = app.Configuration.GetSection(OutputCacheOptions.SectionName).Get<OutputCacheOptions>() ?? new OutputCacheOptions();
        var database = app.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();

        var warning = StalePublicCatalogWarning(
            cache, database, RedisOptions.Resolve(app.Configuration).IsActive, app.Environment.IsDevelopment());

        if (warning is not null)
        {
            app.Logger.LogWarning("{Warning}", warning);
        }
    }
}
