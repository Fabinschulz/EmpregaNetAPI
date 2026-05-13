---
name: sdd-usage-guide-empreganet
description: Como acionar o orquestrador SDD, iterar versões e gerar state.md por feature
version: 1.0.0
date: 2026-05-07
status: Approved
---

# Guia de uso — Orquestrador SDD (EmpregaNet)

Este guia padroniza **como** pedir trabalho de especificação à IA (ou ao fluxo humano+IA), onde gravar ficheiros e **como** fechar uma feature antes da implementação. O comportamento detalhado do orquestrador está em [`SDD-ORCHESTRATOR.md`](SDD-ORCHESTRATOR.md).

**Pasta por feature:** `docs/features/<feature-id>/` (ver [`../features/README.md`](../features/README.md)).

---

## 1. Acionamento inicial (génese v1.0.0)

Os artefactos nascem em **v1.0.0** no frontmatter.

### Opção A — Uma feature atómica

> Orquestrador SDD EmpregaNet: iniciar o escopo de uma nova feature.
>
> **ID:** `[ex.: emp-05-export-relatorio]`
> **Contexto de negócio:** `[problema que resolve]`
> **Regras principais:** `[RBAC, invariantes, limites]`
>
> Cria a pasta da feature e o `prd.md` (v1.0.0) para revisão — sem detalhe técnico de implementação.

### Opção B — Lote pequeno (2–3 features coerentes)

> Orquestrador SDD EmpregaNet: em lote, `[N]` features no mesmo domínio, **uma pasta por feature**. Gera só `prd.md` (v1.0.0) de cada uma para aprovação conjunta.

---

## 2. Iteração e *version bumps*

Ao alterar um artefacto, pedir **explicitamente** a subida de versão no YAML.

| Bump | Quando |
|------|--------|
| **Minor** (v1.1.0) | Novos critérios de aceite, novos campos ou endpoints compatíveis com o existente. |
| **Major** (v2.0.0) | Mudança arquitectural, troca de integração crítica, ou regras de negócio que invalidam o contrato anterior. |

**Modelo de pedido:**

> Actualizar a feature `[ID]`: `[o que mudou no negócio ou no âmbito]`. Actualizar `prd.md` e/ou `design.md` e **subir a versão no frontmatter para vX.Y.Z**; data de hoje.

---

## 3. Congelamento — ficheiro `state.md`

Depois de **PRD, design, spec e tasks** validados e prontos para desenvolvimento, pedir um **resumo congelado**:

> Todas as fases de especificação estão aprovadas. Gera `state.md` em `docs/features/<feature-id>/` com:
>
> 1. Frontmatter YAML (versão consolidada, data, `status: Ready for Development`).
> 2. Tabela de artefactos e versões finais.
> 3. Resumo executivo (um parágrafo).
> 4. Decisões arquitecturais relevantes (ou apontadores para `docs/sdd/adrs/`).

Exemplo mínimo de corpo:

```markdown
## Inventário de artefactos

| Artefacto | Versão | Estado |
|-----------|--------|--------|
| prd.md | 1.0.0 | Approved |
| design.md | 1.0.0 | Approved |
| spec.md | 1.0.0 | Approved |
| tasks.md | 1.0.0 | Open for dev |

## ADRs
- (ou link para docs/sdd/adrs/NNNN-titulo.md)
```

---

## 4. Ligação ao backlog do produto

- Lista de alto nível e prioridades: preferir **ticket/issue tracker** do projecto.
- Opcional: manter um índice textual em `docs/sdd/FEATURES-BACKLOG.md` se a equipa quiser tudo no repo (não substitui issues).

---

## Referências

| Recurso | Path |
|---------|------|
| Orquestrador (fases e gates) | [`SDD-ORCHESTRATOR.md`](SDD-ORCHESTRATOR.md) |
| SDD global | [`EMPREGANET-SDD.md`](EMPREGANET-SDD.md) |
