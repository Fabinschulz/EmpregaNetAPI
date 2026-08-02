# ADRs — Architecture Decision Records

Registos curtos de decisões **estruturais e duradouras** (stack, limites entre serviços, políticas de cache, BFF vs chamada directa, etc.).

## Índice

| ADR | Decisão | Estado |
| --- | ------- | ------ |
| [0001](0001-auth-por-cookies-httponly.md) | Autenticação via cookies httpOnly, sem token acessível por JavaScript | Aceite |
| [0002](0002-rate-limit-unico-por-ip.md) | Rate limit global único; brute-force fica com o lockout do Identity | Aceite |
| [0003](0003-teto-diario-de-emails-por-destinatario.md) | Teto diário de e-mails transacionais por destinatário | Aceite |
| [0004](0004-proxy-confiavel-explicito.md) | Headers de proxy só de proxies explicitamente confiáveis | Aceite |
| [0005](0005-identity-no-dominio.md) | `User`/`Role` herdam de ASP.NET Core Identity dentro do Domain | Aceite |

## Como criar

1. Copiar o template da secção "ADR" em [`../EMPREGANET-SDD.md`](../EMPREGANET-SDD.md).
2. Nomear: `NNNN-titulo-curto.md` (prefixo numérico sequencial na pasta).
3. Ligar a PRs ou *issues* quando existir rastreio externo.

Decisões que só afectam **uma feature** podem viver em `design.md` ou `state.md` dessa feature; promova para aqui quando a decisão for transversal.
