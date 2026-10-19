using System.Net;
using System.Text.Json;
using EmpregaNet.Tests.Support;
using FluentAssertions;

namespace EmpregaNet.Tests.Integration.Proxy;

/// <summary>
/// Comportamento atrás de um proxy ou ALB: o IP real é restaurado só quando o par é confiável, o que o cliente
/// forja não vale, e o rate limit por IP continua contando por cliente.
/// </summary>
public sealed class ForwardedHeadersAndRateLimitTests
{
    private const string AlbPeer = "10.0.1.5";
    private const string InternalSecret = "segredo-interno-do-ssr-com-mais-de-32-caracteres";

    private static Dictionary<string, string?> Trusted(params (string Key, string? Value)[] extra)
    {
        var settings = new Dictionary<string, string?> { ["ForwardedHeaders:KnownNetworks:0"] = "10.0.0.0/16" };
        foreach (var (key, value) in extra)
        {
            settings[key] = value;
        }

        return settings;
    }

    private static Dictionary<string, string?> Limiter(int burst, params (string Key, string? Value)[] extra)
    {
        var settings = new Dictionary<string, string?>
        {
            ["RateLimiting:Enabled"] = "true",
            ["RateLimiting:BurstCapacity"] = burst.ToString(),
            ["RateLimiting:SustainedPerPeriod"] = "1",
            // Período longo: nenhuma ficha volta durante o teste, o balde só esvazia.
            ["RateLimiting:ReplenishmentPeriodInSeconds"] = "3600",
            ["ForwardedHeaders:KnownNetworks:0"] = "10.0.0.0/16"
        };
        foreach (var (key, value) in extra)
        {
            settings[key] = value;
        }

        return settings;
    }

    private static async Task<(string? Ip, string? Scheme)> WhoAsync(ProxyTestApp app, HttpRequestMessage request)
    {
        var response = await app.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (json.RootElement.GetProperty("ip").GetString(), json.RootElement.GetProperty("scheme").GetString());
    }

    // ---- IP real e esquema ----

    [Fact]
    public async Task Proxy_confiavel_por_rede_repassa_o_ip_real_e_o_esquema()
    {
        await using var app = await ProxyTestApp.StartAsync(Trusted());

        var (ip, scheme) = await WhoAsync(app, app.Get("/who", AlbPeer, forwardedFor: "203.0.113.9", forwardedProto: "https"));

        ip.Should().Be("203.0.113.9");
        scheme.Should().Be("https");
    }

    [Fact]
    public async Task Proxy_confiavel_por_ip_exato_repassa_o_ip_real()
    {
        await using var app = await ProxyTestApp.StartAsync(
            new Dictionary<string, string?> { ["ForwardedHeaders:KnownProxies:0"] = AlbPeer });

        var (ip, _) = await WhoAsync(app, app.Get("/who", AlbPeer, forwardedFor: "203.0.113.9"));

        ip.Should().Be("203.0.113.9");
    }

    [Fact]
    public async Task Cliente_que_forja_a_cadeia_atras_do_proxy_so_tem_valida_a_entrada_acrescentada_pelo_proxy()
    {
        await using var app = await ProxyTestApp.StartAsync(Trusted());

        // O cliente real (203.0.113.9) mandou "9.9.9.9"; o ALB acrescenta o IP verdadeiro à direita.
        var (ip, _) = await WhoAsync(app, app.Get("/who", AlbPeer, forwardedFor: "9.9.9.9, 203.0.113.9"));

        ip.Should().Be("203.0.113.9");
    }

    [Fact]
    public async Task Par_fora_da_rede_confiavel_nao_consegue_forjar_ip_nem_esquema()
    {
        await using var app = await ProxyTestApp.StartAsync(Trusted());

        var (ip, scheme) = await WhoAsync(app, app.Get("/who", "198.51.100.4", forwardedFor: "1.2.3.4", forwardedProto: "https"));

        ip.Should().Be("198.51.100.4");
        scheme.Should().Be("http");
    }

    [Fact]
    public async Task Sem_configuracao_os_cabecalhos_encaminhados_sao_ignorados()
    {
        await using var app = await ProxyTestApp.StartAsync(new Dictionary<string, string?>());

        var (ip, scheme) = await WhoAsync(app, app.Get("/who", AlbPeer, forwardedFor: "203.0.113.9", forwardedProto: "https"));

        ip.Should().Be(AlbPeer);
        scheme.Should().Be("http");
    }

    // ---- rate limit atrás do proxy ----

    [Fact]
    public async Task Clientes_distintos_atras_do_mesmo_proxy_tem_baldes_separados()
    {
        await using var app = await ProxyTestApp.StartAsync(Limiter(burst: 3), withRateLimiter: true);

        for (var i = 0; i < 3; i++)
        {
            (await app.Client.SendAsync(app.Get("/who", AlbPeer, forwardedFor: "203.0.113.1"))).StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        (await app.Client.SendAsync(app.Get("/who", AlbPeer, forwardedFor: "203.0.113.1"))).StatusCode
            .Should().Be(HttpStatusCode.TooManyRequests, "o cliente 1 esgotou o próprio balde");

        (await app.Client.SendAsync(app.Get("/who", AlbPeer, forwardedFor: "203.0.113.2"))).StatusCode
            .Should().Be(HttpStatusCode.OK, "o cliente 2 tem balde próprio; o IP do ALB não é a chave");
    }

    [Fact]
    public async Task Cliente_direto_nao_contorna_o_limite_variando_o_x_forwarded_for()
    {
        await using var app = await ProxyTestApp.StartAsync(Limiter(burst: 3), withRateLimiter: true);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 6; i++)
        {
            // Par não confiável: o cabeçalho é ignorado, então o IP é sempre o do par.
            statuses.Add((await app.Client.SendAsync(
                app.Get("/who", "198.51.100.4", forwardedFor: $"203.0.113.{i + 10}"))).StatusCode);
        }

        statuses.Take(3).Should().AllBeEquivalentTo(HttpStatusCode.OK);
        statuses.Skip(3).Should().AllBeEquivalentTo(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Health_check_do_balanceador_nunca_leva_429()
    {
        await using var app = await ProxyTestApp.StartAsync(Limiter(burst: 1), withRateLimiter: true);

        for (var i = 0; i < 10; i++)
        {
            (await app.Client.SendAsync(app.Get("/health/live", AlbPeer))).StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    // ---- chamador interno (SSR) ----

    [Fact]
    public async Task Ssr_com_segredo_nao_depende_do_ip_e_nao_consome_o_balde_dos_visitantes()
    {
        await using var app = await ProxyTestApp.StartAsync(
            Limiter(burst: 2,
                ("RateLimiting:InternalCaller:Secret", InternalSecret),
                ("RateLimiting:InternalCaller:BurstCapacity", "8"),
                ("RateLimiting:InternalCaller:SustainedPerPeriod", "1")),
            withRateLimiter: true);

        // Cada requisição do SSR chega de um IP de tarefa diferente: o IP não importa.
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 8; i++)
        {
            var request = app.Get("/who", AlbPeer, forwardedFor: $"198.51.100.{i + 20}");
            request.Headers.Add("X-Internal-Caller", InternalSecret);
            statuses.Add((await app.Client.SendAsync(request)).StatusCode);
        }

        statuses.Should().AllBeEquivalentTo(HttpStatusCode.OK, "o balde do SSR tem folga própria");

        (await app.Client.SendAsync(app.Get("/who", AlbPeer, forwardedFor: "203.0.113.50"))).StatusCode
            .Should().Be(HttpStatusCode.OK, "um visitante comum continua com o balde dele intacto");
    }

    [Fact]
    public async Task Ssr_autenticado_continua_limitado_e_nao_e_um_bypass_ilimitado()
    {
        await using var app = await ProxyTestApp.StartAsync(
            Limiter(burst: 2,
                ("RateLimiting:InternalCaller:Secret", InternalSecret),
                ("RateLimiting:InternalCaller:BurstCapacity", "4"),
                ("RateLimiting:InternalCaller:SustainedPerPeriod", "1")),
            withRateLimiter: true);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 6; i++)
        {
            var request = app.Get("/who", AlbPeer, forwardedFor: "198.51.100.30");
            request.Headers.Add("X-Internal-Caller", InternalSecret);
            statuses.Add((await app.Client.SendAsync(request)).StatusCode);
        }

        statuses.Take(4).Should().AllBeEquivalentTo(HttpStatusCode.OK);
        statuses.Skip(4).Should().AllBeEquivalentTo(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Segredo_errado_nao_da_privilegio_e_segue_como_cliente_comum()
    {
        await using var app = await ProxyTestApp.StartAsync(
            Limiter(burst: 2, ("RateLimiting:InternalCaller:Secret", InternalSecret)),
            withRateLimiter: true);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 4; i++)
        {
            var request = app.Get("/who", AlbPeer, forwardedFor: "203.0.113.77");
            request.Headers.Add("X-Internal-Caller", "palpite-errado-com-comprimento-suficiente-123");
            statuses.Add((await app.Client.SendAsync(request)).StatusCode);
        }

        statuses.Take(2).Should().AllBeEquivalentTo(HttpStatusCode.OK);
        statuses.Skip(2).Should().AllBeEquivalentTo(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Sem_segredo_configurado_o_cabecalho_do_chamador_interno_e_ignorado()
    {
        await using var app = await ProxyTestApp.StartAsync(Limiter(burst: 2), withRateLimiter: true);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var request = app.Get("/who", AlbPeer, forwardedFor: "203.0.113.88");
            request.Headers.Add("X-Internal-Caller", InternalSecret);
            statuses.Add((await app.Client.SendAsync(request)).StatusCode);
        }

        statuses.Last().Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Segredo_curto_demais_impede_a_aplicacao_de_subir()
    {
        var act = async () => await ProxyTestApp.StartAsync(
            Limiter(burst: 2, ("RateLimiting:InternalCaller:Secret", "curto")), withRateLimiter: true);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*InternalCaller:Secret*32*");
    }
}
