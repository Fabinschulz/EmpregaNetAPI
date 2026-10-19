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
/// </summary>
/// <remarks>
/// Atrás de um ALB, liste a rede das subnets do balanceador (<c>KnownNetworks</c>), nunca IPs de tarefas:
/// os IPs do Fargate mudam a cada início. Com <c>ForwardLimit = 1</c> só a entrada mais à direita de
/// <c>X-Forwarded-For</c> (a que o próprio ALB acrescenta) é usada; o que o cliente forjou antes dela é ignorado.
/// </remarks>
public static class ForwardedHeadersConfig
{
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    /// Menor prefixo aceito para uma rede confiável. Um bloco CIDR de VPC na AWS vai de /16 a /28; uma rede mais
    /// ampla que /16 não é uma rede de balanceador e confiaria em endereços que a infraestrutura não controla.
    /// </summary>
    public const int MinimumIPv4PrefixLength = 16;

    public const int MinimumIPv6PrefixLength = 48;

    public sealed class ForwardedHeadersSettings
    {
        /// <summary>IPs individuais de proxies confiáveis (ex.: "203.0.113.10").</summary>
        public string[] KnownProxies { get; set; } = [];

        /// <summary>Redes confiáveis em notação CIDR (ex.: "10.0.0.0/16").</summary>
        public string[] KnownNetworks { get; set; } = [];

        public bool IsConfigured => KnownProxies.Length > 0 || KnownNetworks.Length > 0;
    }

    /// <summary>
    /// Monta as opções do middleware a partir da configuração, recusando valores inválidos ou amplos demais.
    /// </summary>
    /// <exception cref="InvalidOperationException">IP ou CIDR inválido, ou rede mais ampla que o mínimo aceito.</exception>
    public static ForwardedHeadersOptions BuildOptions(ForwardedHeadersSettings settings)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };

        // O padrão do framework confia em loopback; aqui só entra o que foi configurado de propósito.
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

    /// <summary>Registra o middleware apenas quando há proxies confiáveis configurados.</summary>
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
