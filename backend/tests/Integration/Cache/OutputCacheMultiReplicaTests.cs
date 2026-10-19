using System.Text.Json;
using EmpregaNet.Api.Configuration;
using EmpregaNet.Infra.Persistence.Database;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Tests.Integration.Cache;

/// <summary>
/// Cada "réplica" é uma aplicação com o cache de saída real da API (<see cref="OutputCacheConfig"/>) e o próprio
/// store em memória, como duas tarefas Fargate. O contador mostra quantas vezes o handler rodou: número igual na
/// segunda leitura significa que veio do cache.
/// </summary>
/// <remarks>
/// Caracteriza o limite aceito da Fase 1: sem Redis, o catálogo público só se renova por validade. Requisições
/// autenticadas nunca entram no cache de saída da API, então listas e perfil do usuário não sofrem esse atraso.
/// </remarks>
public sealed class OutputCacheMultiReplicaTests
{
    private sealed class Replica : IAsyncDisposable
    {
        private int _handlerRuns;
        private readonly WebApplication _app;

        public HttpClient Client { get; private set; } = null!;

        private Replica(WebApplication app) => _app = app;

        public static async Task<Replica> StartAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Staging" });
            builder.WebHost.UseTestServer();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
            // Só a configuração do teste: sem isto o appsettings.json local (com Redis ligado) vazaria para o teste.
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OutputCache:DefaultExpirationMinutes"] = "30"
            });
            builder.Services.SetupOutputCache(builder.Configuration);

            var app = builder.Build();
            var replica = new Replica(app);

            app.UseOutputCache();

            app.MapGet("/catalog", () => Results.Json(new { runs = Interlocked.Increment(ref replica._handlerRuns) }))
                .WithMetadata(new OutputCacheAttribute { PolicyName = OutputCachePolicies.PublicCatalog, Tags = ["catalog"] });
            app.MapPost("/write", async (IOutputCacheStore store, CancellationToken ct) =>
            {
                // O que o OutputCacheManager faz depois de uma escrita: invalida por tag, no store LOCAL.
                await store.EvictByTagAsync("catalog", ct);
                return Results.NoContent();
            });

            await app.StartAsync();
            replica.Client = app.GetTestClient();
            return replica;
        }

        public async Task<int> GetRunsAsync()
        {
            var response = await Client.GetAsync("/catalog");
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("runs").GetInt32();
        }

        public async Task WriteAsync() =>
            (await Client.PostAsync("/write", null)).EnsureSuccessStatusCode();

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task Catalogo_publico_e_servido_do_cache_na_segunda_leitura()
    {
        await using var replica = await Replica.StartAsync();

        (await replica.GetRunsAsync()).Should().Be(1);
        (await replica.GetRunsAsync()).Should().Be(1, "segunda leitura servida do cache");
    }

    [Fact]
    public async Task Invalidacao_por_tag_alcanca_so_a_replica_que_atendeu_a_escrita()
    {
        await using var a = await Replica.StartAsync();
        await using var b = await Replica.StartAsync();

        await a.GetRunsAsync();
        await b.GetRunsAsync();

        await a.WriteAsync(); // a escrita chega na réplica A

        (await a.GetRunsAsync()).Should().Be(2, "A invalidou o próprio cache e leu o dado novo");
        (await b.GetRunsAsync()).Should().Be(1,
            "B nunca soube da escrita e serve o catálogo antigo até a validade acabar: limite aceito sem Redis, " +
            "por isso o aviso de inicialização exige validade curta");
    }

    private static string? Warn(int minutes, int replicas = 2, bool redis = false, bool development = false) =>
        OutputCacheConfig.StalePublicCatalogWarning(
            new EmpregaNet.Api.Configuration.OutputCacheOptions { DefaultExpirationMinutes = minutes },
            new DatabaseOptions { ExpectedMaxReplicas = replicas },
            redis,
            development);

    [Fact]
    public void Avisa_quando_ha_varias_replicas_sem_redis_e_validade_longa()
    {
        Warn(90).Should().Contain("90 minuto(s)").And.Contain("2 réplicas").And.Contain("Redis");
    }

    [Theory]
    [InlineData(OutputCacheConfig.MaxSafeExpirationMinutesWithoutSharedStore, 2, false, false)]
    [InlineData(90, 1, false, false)]
    [InlineData(90, 2, true, false)]
    [InlineData(90, 2, false, true)]
    public void Nao_avisa_quando_a_validade_e_curta_ha_uma_replica_ha_redis_ou_e_desenvolvimento(
        int minutes, int replicas, bool redis, bool development)
    {
        Warn(minutes, replicas, redis, development).Should().BeNull();
    }
}
