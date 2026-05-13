# ADRs — Architecture Decision Records

Registos curtos de decisões **estruturais e duradouras** (stack, limites entre serviços, políticas de cache, BFF vs chamada directa, etc.).

## Como criar

1. Copiar o template da secção "ADR" em [`../EMPREGANET-SDD.md`](../EMPREGANET-SDD.md).
2. Nomear: `NNNN-titulo-curto.md` (prefixo numérico sequencial na pasta).
3. Ligar a PRs ou *issues* quando existir rastreio externo.

Decisões que só afectam **uma feature** podem viver em `design.md` ou `state.md` dessa feature; promova para aqui quando a decisão for transversal.
