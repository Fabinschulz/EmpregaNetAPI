# ADR 0002: Rate limit único por IP; brute-force de login fica a cargo do lockout do Identity

## Status
Aceite

## Contexto
Ao endurecer os endpoints de autenticação contra abuso, cogitou-se ter duas políticas de rate limit: uma geral (todos os endpoints) e uma mais estrita, dedicada aos endpoints de auth (login, register, refresh, logout, google, forgot/reset/confirm/resend), sob a justificativa de mitigar brute-force de senha.

Ao revisar, no entanto, o Identity já implementa uma proteção de brute-force mais adequada para esse problema específico: **lockout por conta** (`Lockout.MaxFailedAccessAttempts`), que bloqueia a conta-alvo independentemente do IP de origem do atacante. Um rate limit por IP não protege bem contra brute-force distribuído (múltiplos IPs, botnet) — o lockout por conta protege nesse caso, porque a trava é sobre o alvo, não sobre a origem.

Ter as duas políticas ao mesmo tempo (rate limit dedicado de auth + lockout do Identity) duplicava a responsabilidade de "proteger contra tentativas repetidas de login" em duas camadas com configuração própria, sem ganho real de segurança.

## Decisão
- Existe **uma única política de rate limit**, aplicada a **toda** requisição via `RateLimiterOptions.GlobalLimiter` — sem atributos `[EnableRateLimiting]`/`RequireRateLimiting` espalhados pelos controllers.
- A proteção contra brute-force de senha continua sendo responsabilidade do **lockout do Identity** (por conta, não por IP).

### Revisão 2026-07-25 — token bucket e partição por utilizador

O limite original (janela fixa, 5 req/10s por IP) causou **incidente em produção**: 429 em uso legítimo em todas as telas, exatamente o risco antecipado na secção "Consequências". Reproduzido com 15 requisições paralelas → 5×200 seguidas de 10×429.

Alterações:

| Antes | Agora | Porquê |
| ----- | ----- | ------ |
| Janela fixa | **Token bucket** (`BurstCapacity` + `SustainedPerPeriod`) | A janela fixa não distingue pico legítimo (uma tela que dispara várias chamadas ao carregar) de abuso (taxa sustentada). O balde absorve o pico e limita a taxa. |
| Partição só por IP | **Utilizador autenticado quando há sessão, IP como fallback** | Só por IP, utilizadores atrás do mesmo NAT partilham balde e derrubam-se entre si. |
| 5 req / 10 s | 120 de pico + 60/10 s sustentado (240/120 em Dev) | Ordem de grandeza compatível com uso real. |
| Sem isenção | `RateLimiting:BypassIps` | O SSR do Next.js busca dados a partir do **servidor**: todo esse tráfego chega de um único IP e esgotaria o balde sozinho. |
| — | `RateLimiting:Enabled` | Válvula de escape para desligar em incidente sem recompilar. |

### Revisão 2026-07-26 — tabela de baldes limitada e auto-limpa

Ao avaliar se o desenho acima já bastava, identificou-se um segundo furo: o mecanismo padrão do ASP.NET Core para políticas particionadas (`AddPolicy` + `RateLimitPartition.Get*`) cria um `RateLimiter` por chave de partição e **guarda para sempre** — confirmado inspecionando a documentação e o binário dos pacotes `System.Threading.RateLimiting`/`Microsoft.AspNetCore.RateLimiting` instalados localmente: não existe nenhuma API de limpeza ou teto de tamanho. Cada balde criado (um por IP/utilizador) mantém, além do próprio objeto, um timer de reposição (`AutoReplenishment`) rodando **indefinidamente**, mesmo após a origem parar de mandar tráfego.

Isso torna a própria proteção um vetor de exaustão: uma origem que varia a identidade a cada requisição (rotação de IP, IPv6, ou simplesmente tráfego orgânico disperso da internet) faz a tabela crescer sem fim.

**Correção:** substituição do `AddPolicy` (partitioner-based, cache interno do framework) por um `PartitionedRateLimiter<HttpContext>` próprio (`BoundedEvictingRateLimiter`, em `RateLimiterExtensions.cs`), atribuído via `GlobalLimiter`, com:

1. **Limpeza por ociosidade** — varredura periódica (`EvictionSweepIntervalSeconds`) remove e descarta baldes cheios (sem pedidos) há mais de `IdleEvictionAfterSeconds`; usa a propriedade `IdleDuration` exposta pela própria `RateLimiter` para esse fim.
2. **Teto rígido** (`MaxTrackedPartitions`) — acima do teto, identidades novas caem num balde agregado de excedente (10× a capacidade individual) em vez de ganharem balde próprio, travando o crescimento mesmo dentro da janela de uma única varredura.
3. **Isenções sem estado** (`ExemptPaths`, padrão `["/health"]`) — health checks nunca entram na tabela nem são limitados; um orquestrador/monitor não deve jamais receber 429 de uma sonda de saúde.
4. **Recriação segura** — a corrida rara entre uma requisição e a varredura de limpeza (balde descartado entre resolver a referência e usá-la) é tratada com `catch (ObjectDisposedException)` + recriação e nova tentativa única, tanto no caminho síncrono quanto no assíncrono.

Como o `GlobalLimiter` aplica-se incondicionalmente, deixou de ser necessário `RequireRateLimiting(...)` em `MapControllers()` — removido.

**Verificado (não apenas por leitura de código):**
- Rajada de 400 requisições → mistura de `200`/`429` preservada (comportamento correto mantido após o refactor).
- `/health` sob rajada de 30 requisições → nunca retornou `429` (isenção funcionando).
- Ciclo completo de varredura de limpeza (thresholds reduzidos via variável de ambiente só para o teste) executado sob carga real → sem exceções, sem 500, capacidade plena disponível depois.
- **Não verificado por teste de carga real:** o caminho do teto (`MaxTrackedPartitions`) exigiria milhares de identidades de origem distintas, impraticável a partir de uma única máquina de desenvolvimento sem enfraquecer a config de `ForwardedHeaders` — validado por revisão de código, não por execução.

## Consequências

**Positivas:**
- Uma política, uma configuração, um lugar para ajustar — sem atributos duplicados por controller.
- Cada mecanismo de defesa tem uma responsabilidade clara: rate limit = volume/sobrecarga por origem; lockout = brute-force por conta-alvo.

**Negativas / cuidados:**
- Telas que disparam várias requisições em paralelo no carregamento (dashboard, listagens) podem esbarrar no limite de 5 req/10s em uso legítimo, especialmente logo após login. Se isso ocorrer na prática, o ajuste é subir `PermitLimit` no appsettings (não requer recompilar, só reiniciar a API) — não é para reintroduzir uma segunda política.
- Esse limite é **por IP**: atrás de proxy reverso mal configurado (sem `ForwardedHeaders` — ver ADR 0004), todos os clientes compartilham o IP do proxy e colidem no mesmo balde. Isso é uma falha operacional grave (auto-DoS), não apenas uma limitação aceita.
