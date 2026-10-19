using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmpregaNet.Tests.Support;

/// <summary>Banco compartilhado entre as réplicas simuladas de um teste.</summary>
internal abstract record ReplicaDatabase
{
    public sealed record InMemory(string Name) : ReplicaDatabase;

    public sealed record Postgres(string ConnectionString) : ReplicaDatabase;

    public static ReplicaDatabase NewInMemory() => new InMemory($"replicas_{Guid.NewGuid():N}");
}

/// <summary>
/// Uma "réplica" da API: um contêiner de serviços próprio (nada em memória é compartilhado com as outras),
/// apontando para o mesmo banco. Serve para provar comportamento entre instâncias sem subir o host HTTP.
/// </summary>
internal sealed class ReplicaHost : IDisposable
{
    public ServiceProvider Services { get; }

    private ReplicaHost(ServiceProvider services) => Services = services;

    /// <param name="sharedDataProtection">
    /// <c>true</c> usa a configuração real (<see cref="DataProtectionExtensions"/>); <c>false</c> simula o
    /// padrão do framework num contêiner, com chaves que só existem dentro daquela réplica.
    /// </param>
    public static ReplicaHost Create(
        ReplicaDatabase database,
        IDictionary<string, string?>? settings = null,
        string environment = "Development",
        bool sharedDataProtection = true)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings ?? new Dictionary<string, string?>())
            .Build();

        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(e => e.EnvironmentName).Returns(environment);

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<PostgreSqlContext>(options =>
        {
            switch (database)
            {
                case ReplicaDatabase.InMemory inMemory:
                    options.UseInMemoryDatabase(inMemory.Name);
                    break;
                case ReplicaDatabase.Postgres postgres:
                    options.UseNpgsql(postgres.ConnectionString, npgsql =>
                        npgsql.MigrationsAssembly(typeof(PostgreSqlContext).Assembly.GetName().Name));
                    break;
            }
        });

        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<PostgreSqlContext>()
            .AddDefaultTokenProviders();

        if (sharedDataProtection)
        {
            services.AddEmpregaNetDataProtection(configuration, hostEnvironment.Object);
        }
        else
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
        }

        return new ReplicaHost(services.BuildServiceProvider());
    }

    public void Dispose() => Services.Dispose();

    public IServiceScope Scope() => Services.CreateScope();

    /// <summary>PFX autoassinado, em Base64, para os testes de proteção em repouso.</summary>
    public static string CreateCertificatePfxBase64(string password, string subject = "CN=empreganet-dp-test")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, password));
    }
}
