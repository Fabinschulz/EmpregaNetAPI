using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Ajustes do limite de requisições.
///
/// <para><b>Como funciona, em uma analogia:</b> cada usuário tem um balde de fichas. Toda
/// requisição gasta 1 ficha. O balde nasce cheio e vai sendo reabastecido aos poucos. Se o
/// usuário gastar mais rápido do que o reabastecimento, o balde seca e ele passa a receber
/// erro 429 até o balde encher de novo.</para>
///
/// <para><b>Por que um balde, e não "X requisições a cada Y segundos":</b> abrir uma tela
/// costuma disparar várias chamadas ao mesmo tempo (a listagem, os contadores, o perfil...).
/// Isso é uso normal, e uma regra rígida barraria. O balde tem folga guardada para absorver
/// esse pico, mas continua limitando o <i>ritmo contínuo</i>, que é o que caracteriza abuso.
/// Resumindo: pico ocasional passa, enxurrada constante não.</para>
/// </summary>
public sealed class RateLimit
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Liga/desliga o limite inteiro. Serve como válvula de escape: se algo der errado em
    /// produção, dá para desativar mexendo só no appsettings, sem recompilar.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Tamanho do balde: quantas requisições um mesmo usuário consegue disparar de uma vez,
    /// aproveitando as fichas acumuladas. É o que permite uma tela carregar vários dados em
    /// paralelo sem tomar 429.
    /// </summary>
    public int BurstCapacity { get; set; } = 120;

    /// <summary>
    /// Quantas fichas voltam para o balde a cada período. É isto que define o ritmo que o
    /// usuário consegue manter de forma contínua (o pico acima é só a folga inicial).
    /// </summary>
    public int SustainedPerPeriod { get; set; } = 60;

    /// <summary>De quanto em quanto tempo as fichas são repostas.</summary>
    public int ReplenishmentPeriodInSeconds { get; set; } = 10;

    /// <summary>
    /// Quantas requisições ficam esperando na fila quando o balde está vazio. Zero significa
    /// "recusa na hora", que é o desejado numa API: é melhor o cliente receber 429 e poder
    /// tentar de novo do que ficar pendurado esperando.
    /// </summary>
    public int QueueLimit { get; set; } = 0;

    /// <summary>
    /// Endereços de rede que passam sem limite algum.
    ///
    /// <para>Necessário para origens confiáveis que concentram o tráfego de muita gente num
    /// único endereço. O caso concreto aqui é o servidor do site (Next.js): quando ele monta
    /// as páginas públicas de vagas, é <i>ele</i> quem chama a API, não o navegador de cada
    /// visitante. Sem isenção, o tráfego de todos os visitantes cairia num balde só e o
    /// esgotaria sozinho.</para>
    /// </summary>
    public string[] BypassIps { get; set; } = [];

    /// <summary>
    /// Rotas que nunca são limitadas (comparação por início do caminho).
    ///
    /// <para>"/health" vem por padrão porque quem chama essa rota é o monitoramento, não um
    /// usuário. Se ela levasse 429, o monitor concluiria que o serviço caiu justamente quando
    /// ele está apenas movimentado.</para>
    /// </summary>
    public string[] ExemptPaths { get; set; } = ["/health"];

    /// <summary>
    /// Quantos usuários/endereços diferentes podem ter balde próprio ao mesmo tempo.
    ///
    /// <para>Existe para conter um abuso específico: alguém que troca de endereço a cada
    /// requisição criaria um balde novo por chamada e faria a tabela crescer sem parar. Ao
    /// bater neste teto, quem chega novo deixa de ganhar balde individual e passa a dividir
    /// um balde coletivo, ou seja, a memória para de crescer.</para>
    /// </summary>
    public int MaxTrackedPartitions { get; set; } = 20_000;

    /// <summary>
    /// Quanto tempo um balde precisa ficar parado (cheio, sem ninguém pedindo) para ser
    /// jogado fora e liberar memória.
    /// </summary>
    public int IdleEvictionAfterSeconds { get; set; } = 600;

    /// <summary>De quanto em quanto tempo a limpeza dos baldes parados é executada.</summary>
    public int EvictionSweepIntervalSeconds { get; set; } = 60;
}

public static class RateLimiterExtensions
{
    /// <summary>
    /// Registra o limite de requisições da API: quem conta as fichas e o que responder quando
    /// elas acabam.
    /// </summary>
    public static IServiceCollection SetupRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimit.SectionName).Get<RateLimit>() ?? new RateLimit();
        var bypassIps = ParseBypassIps(options.BypassIps);

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // `GlobalLimiter` vale para toda requisição automaticamente. A alternativa seria
            // marcar endpoint por endpoint (`RequireRateLimiting`/`[EnableRateLimiting]`), o
            // que é fácil de esquecer numa rota nova, e uma rota esquecida fica sem proteção.
            rateLimiterOptions.GlobalLimiter = new BoundedEvictingRateLimiter(options, bypassIps);

            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var response = httpContext.Response;

                if (response.HasStarted)
                {
                    return;
                }

                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                    : options.ReplenishmentPeriodInSeconds;

                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                response.ContentType = "application/json";

    
                var error = new DomainError
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Code = DomainErrorEnum.TOO_MANY_REQUESTS,
                    Message = $"Muitas requisições em pouco tempo. Tente novamente em {retryAfterSeconds} segundo(s).",
                    Details = new { retryAfterSeconds },
                    CorrelationId = httpContext.Items["Correlation-ID"]?.ToString() ?? httpContext.TraceIdentifier
                };

                await response.WriteAsJsonAsync(error, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>Converte a lista de IPs do appsettings, descartando valores inválidos.</summary>
    private static HashSet<IPAddress> ParseBypassIps(IEnumerable<string> raw)
    {
        var parsed = new HashSet<IPAddress>();

        foreach (var candidate in raw)
        {
            if (IPAddress.TryParse(candidate?.Trim(), out var ip) && NormalizeIp(ip) is { } normalized)
            {
                parsed.Add(normalized);
            }
        }

        return parsed;
    }

    /// <summary>
    /// Deixa o endereço num formato único para comparação. O mesmo computador pode aparecer
    /// como "127.0.0.1" ou como "::ffff:127.0.0.1" (IPv4 embrulhado em IPv6); sem normalizar,
    /// os dois seriam tratados como origens diferentes.
    /// </summary>
    internal static IPAddress? NormalizeIp(IPAddress? ip) =>
        ip is null ? null : ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
}

/// <summary>
/// Guarda um balde de fichas por identidade (usuário logado ou, se anônimo, endereço de rede)
/// e garante que essa coleção de baldes <b>não cresça sem controle</b>.
///
/// <para><b>Por que existe esta classe em vez do mecanismo pronto do ASP.NET Core:</b> o
/// método padrão (<c>AddPolicy</c> + <c>RateLimitPartition</c>) cria um balde por identidade e
/// <b>guarda para sempre</b>, não existe nenhuma opção de expirar ou limitar a quantidade
/// (confirmado na documentação e no binário dos pacotes <c>System.Threading.RateLimiting</c> e
/// <c>Microsoft.AspNetCore.RateLimiting</c>). Cada balde ainda mantém um cronômetro próprio
/// rodando para repor as fichas. Ou seja: quem trocasse de endereço a cada requisição criaria
/// baldes eternos até esgotar a memória do servidor, a proteção viraria o próprio ataque.</para>
///
/// <para><b>As três travas:</b></para>
/// <para>1. <b>Limpeza automática</b>: de tempo em tempo, baldes parados (cheios e sem
/// ninguém pedindo) são descartados e liberam memória.</para>
/// <para>2. <b>Teto de baldes</b>: passando do limite de identidades simultâneas, quem chega
/// novo divide um balde coletivo em vez de ganhar um próprio. Isso segura o crescimento até
/// numa rajada rápida, entre duas limpezas.</para>
/// <para>3. <b>Isenções sem memória</b>: rotas de monitoramento e endereços confiáveis usam
/// um limitador único compartilhado e nunca entram na tabela.</para>
/// </summary>
internal sealed class BoundedEvictingRateLimiter : PartitionedRateLimiter<HttpContext>
{
    private readonly RateLimit _options;
    private readonly IReadOnlySet<IPAddress> _bypassIps;

    /// <summary>Um balde por identidade. É esta tabela que precisa de teto e limpeza.</summary>
    private readonly ConcurrentDictionary<string, RateLimiter> _limiters = new();

    /// <summary>
    /// Contagem própria de baldes na tabela.
    ///
    /// <para>Usar <c>_limiters.Count</c> aqui seria uma armadilha: no
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> essa propriedade tranca o dicionário
    /// inteiro para contar. E ela só seria consultada quando a identidade é nova - exatamente
    /// o que acontece a cada requisição num ataque com muitos endereços. A verificação ficaria
    /// mais lenta justamente na hora em que ela mais importa.</para>
    /// </summary>
    private int _trackedCount;

    /// <summary>Limitador "sem limite", usado pelas isenções. Um só, compartilhado.</summary>
    private readonly RateLimiter _noopLimiter = RateLimitPartition.GetNoLimiter("bypass").Factory("bypass");

    /// <summary>
    /// Balde coletivo de quem passou do teto. Mais largo que o individual, porque representa
    /// a soma de muitas identidades, mas ainda limitado, senão o teto não protegeria nada.
    /// </summary>
    private readonly RateLimiter _overflowLimiter;

    private readonly Timer _evictionTimer;

    /// <summary>`volatile` porque é escrito na thread que descarta e lido na thread do timer.</summary>
    private volatile bool _disposed;

    public BoundedEvictingRateLimiter(RateLimit options, IReadOnlySet<IPAddress> bypassIps)
    {
        _options = options;
        _bypassIps = bypassIps;

        _overflowLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = options.BurstCapacity * 10,
            TokensPerPeriod = options.SustainedPerPeriod * 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(options.ReplenishmentPeriodInSeconds),
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });

        var sweepInterval = TimeSpan.FromSeconds(Math.Max(1, options.EvictionSweepIntervalSeconds));
        _evictionTimer = new Timer(_ => EvictIdlePartitions(), null, sweepInterval, sweepInterval);
    }

    /// <summary>Tenta gastar uma ficha do balde de quem fez esta requisição.</summary>
    protected override RateLimitLease AttemptAcquireCore(HttpContext resource, int permitCount)
    {
        var (key, limiter) = ResolveLimiter(resource);

        try
        {
            return limiter.AttemptAcquire(permitCount);
        }
        catch (ObjectDisposedException) when (key is not null)
        {
            return GetOrCreate(key).AttemptAcquire(permitCount);
        }
    }

    /// <summary>Versão assíncrona de <see cref="AttemptAcquireCore"/>.</summary>
    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(
        HttpContext resource, int permitCount, CancellationToken cancellationToken)
    {
        var (key, limiter) = ResolveLimiter(resource);

        try
        {
            return await limiter.AcquireAsync(permitCount, cancellationToken);
        }
        catch (ObjectDisposedException) when (key is not null)
        {
            return await GetOrCreate(key).AcquireAsync(permitCount, cancellationToken);
        }
    }

    /// <summary>
    /// Fotografia do estado do balde desta identidade, para diagnóstico.
    ///
    /// <para>Diferente dos métodos acima, <b>nunca cria</b> um balde: é uma consulta, e o
    /// contrato do framework diz "as estatísticas, <i>se disponíveis</i>". Se criasse, uma
    /// simples tela de monitoramento consultando identidades encheria a tabela, o mesmo
    /// crescimento que esta classe existe para evitar.</para>
    /// </summary>
    public override RateLimiterStatistics? GetStatistics(HttpContext resource)
    {
        if (!_options.Enabled || IsExempt(resource))
        {
            return _noopLimiter.GetStatistics();
        }

        return _limiters.TryGetValue(GetPartitionKey(resource), out var limiter)
            ? limiter.GetStatistics()
            : null;
    }

    /// <summary>
    /// Escolhe qual balde vale para esta requisição, sem deixar a tabela crescer sem controle:
    ///
    /// <para>• isento (rota de monitoramento ou IP confiável) -> balde compartilhado sem limite;</para>
    /// <para>• identidade já conhecida -> o balde dela;</para>
    /// <para>• identidade nova e ainda há espaço -> cria o balde dela;</para>
    /// <para>• identidade nova e o teto estourou -> balde coletivo.</para>
    ///
    /// <para>A chave volta como <c>null</c> nos casos de balde compartilhado, para sinalizar a
    /// quem chamou que ali não existe risco de "balde descartado" (eles nunca são limpos).</para>
    /// </summary>
    private (string? Key, RateLimiter Limiter) ResolveLimiter(HttpContext httpContext)
    {
        if (!_options.Enabled || IsExempt(httpContext))
        {
            return (null, _noopLimiter);
        }

        var key = GetPartitionKey(httpContext);

        if (_limiters.TryGetValue(key, out var existing))
        {
            return (key, existing);
        }

        if (Volatile.Read(ref _trackedCount) >= _options.MaxTrackedPartitions)
        {
            return (null, _overflowLimiter);
        }

        return (key, GetOrCreate(key));
    }

    /// <summary>
    /// Devolve o balde desta identidade, criando se ainda não existir.
    ///
    /// <para>Usa <c>TryAdd</c> em vez de <c>GetOrAdd</c>/<c>AddOrUpdate</c> de propósito: se
    /// duas requisições da mesma identidade chegarem juntas, as duas podem construir um balde,
    /// mas só um entra na tabela. O perdedor precisa ser <b>descartado explicitamente</b>,
    /// senão o cronômetro de reposição dele continuaria rodando à solta, que é exatamente o
    /// vazamento que esta classe combate. O contador só é incrementado por quem realmente
    /// inseriu, para não contar baldes que foram jogados fora.</para>
    /// </summary>
    private RateLimiter GetOrCreate(string key)
    {
        var candidate = CreateLimiter();

        if (_limiters.TryAdd(key, candidate))
        {
            Interlocked.Increment(ref _trackedCount);
            return candidate;
        }

        candidate.Dispose();

        return _limiters.TryGetValue(key, out var winner) ? winner : _overflowLimiter;
    }

    private TokenBucketRateLimiter CreateLimiter() => new(new TokenBucketRateLimiterOptions
    {
        TokenLimit = _options.BurstCapacity,
        TokensPerPeriod = _options.SustainedPerPeriod,
        ReplenishmentPeriod = TimeSpan.FromSeconds(_options.ReplenishmentPeriodInSeconds),
        AutoReplenishment = true,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = _options.QueueLimit
    });

    /// <summary>Diz se esta requisição passa sem limite (rota isenta ou endereço confiável).</summary>
    private bool IsExempt(HttpContext httpContext)
    {
        foreach (var path in _options.ExemptPaths)
        {
            if (httpContext.Request.Path.StartsWithSegments(path))
            {
                return true;
            }
        }

        if (_bypassIps.Count == 0)
        {
            return false;
        }

        var ip = RateLimiterExtensions.NormalizeIp(httpContext.Connection.RemoteIpAddress);
        return ip is not null && _bypassIps.Contains(ip);
    }

    /// <summary>
    /// Identifica de quem é o balde: do <b>usuário logado</b>, quando há sessão; do endereço
    /// de rede, quando é visitante anônimo.
    ///
    /// <para>Separar por usuário importa porque, contando só por endereço, várias pessoas na
    /// mesma rede (empresa, operadora de celular) dividiriam um balde só e se derrubariam
    /// entre si sem ter feito nada de errado.</para>
    /// </summary>
    private static string GetPartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue("userId")
                     ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        var ip = RateLimiterExtensions.NormalizeIp(httpContext.Connection.RemoteIpAddress);
        return ip is null ? "ip:unknown" : $"ip:{ip}";
    }

    /// <summary>
    /// Limpeza periódica: joga fora os baldes que estão parados e libera a memória deles.
    ///
    /// <para><c>IdleDuration</c> só tem valor quando o balde está cheio e sem ninguém pedindo,
    /// então descartar nessa situação é seguro: não há requisição em andamento nem fichas
    /// gastas a "esquecer". Se ainda assim uma requisição pegar a referência no exato instante
    /// do descarte, os métodos de aquisição recriam o balde e seguem.</para>
    /// </summary>
    private void EvictIdlePartitions()
    {
        if (_disposed)
        {
            return;
        }

        var threshold = TimeSpan.FromSeconds(_options.IdleEvictionAfterSeconds);

        foreach (var (key, limiter) in _limiters)
        {
            if (limiter.IdleDuration is { } idle && idle >= threshold && _limiters.TryRemove(key, out var removed))
            {
                Interlocked.Decrement(ref _trackedCount);
                removed.Dispose();
            }
        }
    }

    /// <summary>Encerra o cronômetro de limpeza e descarta todos os baldes.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _evictionTimer.Dispose();
            _noopLimiter.Dispose();
            _overflowLimiter.Dispose();

            foreach (var limiter in _limiters.Values)
            {
                limiter.Dispose();
            }

            _limiters.Clear();
            Volatile.Write(ref _trackedCount, 0);
        }

        base.Dispose(disposing);
    }
}
