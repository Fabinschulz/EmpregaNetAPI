using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace EmpregaNet.Api.Configuration;

/// <summary>
/// Suporte opcional a proxy reverso (X-Forwarded-For / X-Forwarded-Proto), seguro por padrão.
///
/// Sem proxies configurados, o middleware NÃO é registrado e todo header X-Forwarded-* é ignorado
/// (comportamento correto para execução direta, ex.: localhost). No deploy atrás de proxy/load
/// balancer, preencha <c>ForwardedHeaders:KnownProxies</c> ou <c>KnownNetworks</c> no appsettings —
/// só os proxies listados são confiáveis, impedindo spoofing de IP via X-Forwarded-For
/// (que burlaria o rate limiter por IP). Nunca use ASPNETCORE_FORWARDEDHEADERS_ENABLED=true:
/// essa env vai limpa as listas e passa a confiar em qualquer origem.
/// Ref.: https://learn.microsoft.com/aspnet/core/breaking-changes/8/forwarded-headers-unknown-proxies
/// </summary>
public static class ForwardedHeadersConfig
{
    public const string SectionName = "ForwardedHeaders";

    public sealed class ForwardedHeadersSettings
    {
        /// <summary>IPs individuais de proxies confiáveis (ex.: "203.0.113.10").</summary>
        public string[] KnownProxies { get; set; } = [];

        /// <summary>Redes confiáveis em notação CIDR (ex.: "10.0.0.0/8").</summary>
        public string[] KnownNetworks { get; set; } = [];
    }

    /// <summary>Registra o middleware apenas quando há proxies confiáveis configurados.</summary>
    public static WebApplication UseForwardedHeadersIfConfigured(this WebApplication app)
    {
        var settings = app.Configuration.GetSection(SectionName).Get<ForwardedHeadersSettings>()
            ?? new ForwardedHeadersSettings();

        if (settings.KnownProxies.Length == 0 && settings.KnownNetworks.Length == 0)
        {
            return app;
        }

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        foreach (var proxy in settings.KnownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy.Trim()));
        }

        foreach (var network in settings.KnownNetworks)
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network.Trim()));
        }

        app.UseForwardedHeaders(options);
        return app;
    }
}
