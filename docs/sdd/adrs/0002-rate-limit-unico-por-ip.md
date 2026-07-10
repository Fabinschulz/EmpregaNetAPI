# ADR 0002: Rate limit único por IP; brute-force de login fica a cargo do lockout do Identity

## Status
Aceite

## Contexto
Ao endurecer os endpoints de autenticação contra abuso, cogitou-se ter duas políticas de rate limit: uma geral (todos os endpoints) e uma mais estrita, dedicada aos endpoints de auth (login, register, refresh, logout, google, forgot/reset/confirm/resend), sob a justificativa de mitigar brute-force de senha.

Ao revisar, no entanto, o Identity já implementa uma proteção de brute-force mais adequada para esse problema específico: **lockout por conta** (`Lockout.MaxFailedAccessAttempts`), que bloqueia a conta-alvo independentemente do IP de origem do atacante. Um rate limit por IP não protege bem contra brute-force distribuído (múltiplos IPs, botnet) — o lockout por conta protege nesse caso, porque a trava é sobre o alvo, não sobre a origem.

Ter as duas políticas ao mesmo tempo (rate limit dedicado de auth + lockout do Identity) duplicava a responsabilidade de "proteger contra tentativas repetidas de login" em duas camadas com configuração própria, sem ganho real de segurança.

## Decisão
- Existe **uma única política de rate limit** (`FixedWindowPolicy`), particionada por IP (`RateLimitPartition.GetFixedWindowLimiter` usando `Connection.RemoteIpAddress`), aplicada a todos os endpoints via `RequireRateLimiting` global — sem atributos `[EnableRateLimiting]` espalhados pelos controllers.
- Limite atual: 5 requisições / 10 segundos por IP (configurável em `RateLimiting:PermitLimit`/`WindowInSeconds` no appsettings, sem necessidade de recompilar).
- A proteção contra brute-force de senha continua sendo responsabilidade do **lockout do Identity** (por conta, não por IP).

## Consequências

**Positivas:**
- Uma política, uma configuração, um lugar para ajustar — sem atributos duplicados por controller.
- Cada mecanismo de defesa tem uma responsabilidade clara: rate limit = volume/sobrecarga por origem; lockout = brute-force por conta-alvo.

**Negativas / cuidados:**
- Telas que disparam várias requisições em paralelo no carregamento (dashboard, listagens) podem esbarrar no limite de 5 req/10s em uso legítimo, especialmente logo após login. Se isso ocorrer na prática, o ajuste é subir `PermitLimit` no appsettings (não requer recompilar, só reiniciar a API) — não é para reintroduzir uma segunda política.
- Esse limite é **por IP**: atrás de proxy reverso mal configurado (sem `ForwardedHeaders` — ver ADR 0004), todos os clientes compartilham o IP do proxy e colidem no mesmo balde. Isso é uma falha operacional grave (auto-DoS), não apenas uma limitação aceita.
