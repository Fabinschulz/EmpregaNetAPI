using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace EmpregaNet.Api.Configuration;

/// <summary>
/// Suporte opcional a proxy reverso (X-Forwarded-For / X-Forwarded-Proto), seguro por padrão.
///
/// Sem proxies configurados, o middleware NÃO é registrado e todo header X-Forwarded-* é ignorado
/// (comportamento correto para execução direta, ex.: localhost). No deploy atrás de proxy/load
/// balancer, preencha <c>ForwardedHeaders:KnownProxies</c> ou <c>KnownNetworks</c> no appsettings
/// só os proxies listados são confiáveis, impedindo spoofing de IP via X-Forwarded-For
/// (que burlaria o rate limiter por IP). Nunca use ASPNETCORE_FORWARDEDHEADERS_ENABLED=true:
/// essa env vai limpa as listas e passa a confiar em qualquer origem.
/// Ref.: https://learn.microsoft.com/aspnet/core/breaking-changes/8/forwarded-headers-unknown-proxies
/// Atrás de um ALB, liste a rede das subnets do balanceador (<c>KnownNetworks</c>), nunca IPs de tarefas.
/// </summary>
public static class ForwardedHeadersConfig
{
    public const string SectionName = "ForwardedHeaders";

    // Uma VPC na AWS vai de /16 a /28; rede mais ampla confiaria em endereços que a infraestrutura não controla.
    public const int MinimumIPv4PrefixLength = 16;

    public const int MinimumIPv6PrefixLength = 48;

    public sealed class ForwardedHeadersSettings
    {
        public string[] KnownProxies { get; set; } = [];

        /// <summary>Notação CIDR, ex.: "10.0.0.0/16".</summary>
        public string[] KnownNetworks { get; set; } = [];

        public bool IsConfigured => KnownProxies.Length > 0 || KnownNetworks.Length > 0;
    }

    /// <exception cref="InvalidOperationException">IP ou CIDR inválido, ou rede mais ampla que o mínimo aceito.</exception>
    public static ForwardedHeadersOptions BuildOptions(ForwardedHeadersSettings settings)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };

        // O padrão do framework confia em loopback.
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        for (var i = 0; i < settings.KnownProxies.Length; i++)
        {
            var raw = settings.KnownProxies[i]?.Trim();
            if (!IPAddress.TryParse(raw, out var proxy))
            {
                throw new InvalidOperationException(
                    $"'{SectionName}:KnownProxies:{i}' não é um IP válido: '{raw}'.");
            }

            options.KnownProxies.Add(proxy);
        }

        for (var i = 0; i < settings.KnownNetworks.Length; i++)
        {
            var raw = settings.KnownNetworks[i]?.Trim();
            if (!System.Net.IPNetwork.TryParse(raw, out var network))
            {
                throw new InvalidOperationException(
                    $"'{SectionName}:KnownNetworks:{i}' não é um CIDR válido: '{raw}'.");
            }

            var minimum = network.BaseAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                ? MinimumIPv6PrefixLength
                : MinimumIPv4PrefixLength;

            if (network.PrefixLength < minimum)
            {
                throw new InvalidOperationException(
                    $"'{SectionName}:KnownNetworks:{i}' ('{raw}') é ampla demais: o prefixo mínimo aceito é /{minimum}. " +
                    "Liste só a rede das subnets do balanceador.");
            }

            options.KnownIPNetworks.Add(network);
        }

        return options;
    }

    public static WebApplication UseForwardedHeadersIfConfigured(this WebApplication app)
    {
        var settings = app.Configuration.GetSection(SectionName).Get<ForwardedHeadersSettings>()
            ?? new ForwardedHeadersSettings();

        if (!settings.IsConfigured)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.Logger.LogWarning(
                    "ForwardedHeaders não configurado: X-Forwarded-* é ignorado. Atrás de um proxy ou ALB, o limite " +
                    "de requisições por IP enxergaria o IP do proxy para todos os clientes (ADR 0004).");
            }

            return app;
        }

        app.UseForwardedHeaders(BuildOptions(settings));
        return app;
    }
}
