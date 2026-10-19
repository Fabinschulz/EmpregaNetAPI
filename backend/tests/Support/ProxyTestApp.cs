using System.Net;
using EmpregaNet.Api.Configuration;
using EmpregaNet.Infra.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Tests.Support;

/// <summary>
/// Mini-aplicação com a mesma ordem de middlewares da API (ForwardedHeaders antes do rate limiter), rodando em
/// TestServer. O endereço do par TCP, que o TestServer não tem, vem do cabeçalho de teste <c>X-Test-Peer</c>.
/// </summary>
internal sealed class ProxyTestApp : IAsyncDisposable
{
    public const string PeerHeader = "X-Test-Peer";

    private readonly WebApplication _app;

    public HttpClient Client { get; }

    private ProxyTestApp(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public static async Task<ProxyTestApp> StartAsync(IDictionary<string, string?> settings, bool withRateLimiter = false)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Staging" });
        builder.WebHost.UseTestServer();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Configuration.AddInMemoryCollection(settings);

        if (withRateLimiter)
        {
            builder.Services.SetupRateLimiter(builder.Configuration);
        }

        var app = builder.Build();

        app.Use((context, next) =>
        {
            if (context.Request.Headers.TryGetValue(PeerHeader, out var peer))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(peer.ToString());
            }

            return next();
        });

        app.UseForwardedHeadersIfConfigured();

        if (withRateLimiter)
        {
            app.UseRateLimiter();
        }

        app.MapGet("/who", (HttpContext context) => Results.Json(new
        {
            ip = context.Connection.RemoteIpAddress?.ToString(),
            scheme = context.Request.Scheme
        }));
        app.MapGet("/health/live", () => Results.Ok());

        await app.StartAsync();
        return new ProxyTestApp(app);
    }

    public HttpRequestMessage Get(string path, string peer, string? forwardedFor = null, string? forwardedProto = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add(PeerHeader, peer);

        if (forwardedFor is not null)
        {
            request.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        if (forwardedProto is not null)
        {
            request.Headers.Add("X-Forwarded-Proto", forwardedProto);
        }

        return request;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
