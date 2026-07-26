# ADR 0002: Rate limit único por IP; brute-force de login fica a cargo do lockout do Identity

## Status
Aceite

## Contexto
Ao endurecer os endpoints de autenticação contra abuso, cogitou-se ter duas políticas de rate limit: uma geral (todos os endpoints) e uma mais estrita, dedicada aos endpoints de auth (login, register, refresh, logout, google, forgot/reset/confirm/resend), sob a justificativa de mitigar brute-force de senha.

Ao revisar, no entanto, o Identity já implementa uma proteção de brute-force mais adequada para esse problema específico: **lockout por conta** (`Lockout.MaxFailedAccessAttempts`), que bloqueia a conta-alvo independentemente do IP de origem do atacante. Um rate limit por IP não protege bem contra brute-force distribuído (múltiplos IPs, botnet) — o lockout por conta protege nesse caso, porque a trava é sobre o alvo, não sobre a origem.

Ter as duas políticas ao mesmo tempo (rate limit dedicado de auth + lockout do Identity) duplicava a responsabilidade de "proteger contra tentativas repetidas de login" em duas camadas com configuração própria, sem ganho real de segurança.

## Decisão
- Existe **uma única política de rate limit** (`GlobalPolicy`), aplicada a todos os endpoints via `RequireRateLimiting` global — sem atributos `[EnableRateLimiting]` espalhados pelos controllers.
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


## Consequências

**Positivas:**
- Uma política, uma configuração, um lugar para ajustar — sem atributos duplicados por controller.
- Cada mecanismo de defesa tem uma responsabilidade clara: rate limit = volume/sobrecarga por origem; lockout = brute-force por conta-alvo.

**Negativas / cuidados:**
- Telas que disparam várias requisições em paralelo no carregamento (dashboard, listagens) podem esbarrar no limite de 5 req/10s em uso legítimo, especialmente logo após login. Se isso ocorrer na prática, o ajuste é subir `PermitLimit` no appsettings (não requer recompilar, só reiniciar a API) — não é para reintroduzir uma segunda política.
- Esse limite é **por IP**: atrás de proxy reverso mal configurado (sem `ForwardedHeaders` — ver ADR 0004), todos os clientes compartilham o IP do proxy e colidem no mesmo balde. Isso é uma falha operacional grave (auto-DoS), não apenas uma limitação aceita.
