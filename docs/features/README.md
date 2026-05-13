# Features — especificação SDD por pasta

Este directório guarda **uma pasta por incremento de produto** quando queres o fluxo **especificar antes de codificar**, com artefactos rastreáveis e revisão fase a fase.

**Índice geral do `docs/`** (mapa do repo EmpregaNet): [`../README.md`](../README.md).

| Documento global | Link |
| ---------------- | ---- |
| Orquestrador (regras, gate, conteúdo mínimo por ficheiro) | [`../sdd/SDD-ORCHESTRATOR.md`](../sdd/SDD-ORCHESTRATOR.md) |
| Como acionar a IA, versionar e gerar `state.md` | [`../sdd/SDD-USAGE-GUIDE.md`](../sdd/SDD-USAGE-GUIDE.md) |
| SDD EmpregaNet (fases A–E, princípios, gates) | [`../sdd/EMPREGANET-SDD.md`](../sdd/EMPREGANET-SDD.md) |
| Skill Cursor (entrada única para o fluxo) | [`../skills/sdd-orchestrator/SKILL.md`](../skills/sdd-orchestrator/SKILL.md) |

---

## Quando criar uma pasta aqui

- Feature **nova** com critérios de aceite e impacto em API, modelo ou UI relevante.
- Refactor **grande** onde o contrato negócio/técnico precisa de ficar escrito antes do código.

Não é obrigatório para cada bugfix ou ajuste pequeno: nesses casos basta ticket + PR com critérios no corpo.

---

## Estrutura canónica

```text
docs/features/
└── <feature-id>/
    ├── prd.md       # negócio, RBAC, critérios de aceite — sem solução técnica
    ├── design.md    # contratos, HTTP, mermaid, infra alinhada ao repo
    ├── spec.md      # matriz CA → cobertura (enxuto, sem duplicar design)
    ├── tasks.md     # plano de implementação + deviation notes
    └── state.md     # opcional: freeze "Ready for Development"
```

Fluxo obrigatório: **prd → design → spec + tasks** (uma fase **aprovada** de cada vez). Detalhes e *gate* anti-código: orquestrador.

---

## Convenção `feature-id`

| Regra | Exemplo |
| ----- | ------- |
| Minúsculas, palavras separadas por `-` | `emp-14-notificar-candidatura` |
| Prefixo opcional por domínio (`cand`, `emp`, `org`, …) | `cand-03-filtro-localizacao` |
| Um ID = um âmbito; não reciclar | — |

Preferir IDs estáveis ao longo da vida da spec (mudar só se o âmbito da feature mudar de forma material).

---

## Artefactos (resumo)

| Ficheiro | Fase SDD EmpregaNet | O que vai aqui |
| -------- | ------------------- | ---------------- |
| `prd.md` | A — descoberta / especificação | Problema, personas, **critérios de aceite**, non-goals. **Proibido:** EF, NuGet, Redis, endpoints. |
| `design.md` | B — desenho técnico | Modelos, DTOs, rotas HTTP, exemplos JSON, fluxos (`mermaid`), auth/cache/mensagens se aplicável. |
| `spec.md` | preparação C | Tabela CA → nome/classe de teste ou tipo de teste; gaps de cobertura. |
| `tasks.md` | preparação C | Tarefas ordenadas (domínio → application → infra → API/UI); secção **Deviation notes** durante o dev. |
| `state.md` | *freeze* opcional | Versões finais, resumo executivo, apontadores ADR; ver guia de uso. |

Todos devem começar com **frontmatter** (`version`, `date`, `status`) como em [`../sdd/SDD-ORCHESTRATOR.md`](../sdd/SDD-ORCHESTRATOR.md).

---

## O que não pertence à pasta da feature

| Conteúdo | Onde colocar |
| -------- | ------------ |
| Decisão arquitectural **transversal** | [`../sdd/adrs/`](../sdd/adrs/README.md) |
| Inventário opcional de linhas de produto | [`../sdd/FEATURES-BACKLOG.md`](../sdd/FEATURES-BACKLOG.md) ou *issue tracker* |
| Filosofia SDD e gates de CI/review | [`../sdd/EMPREGANET-SDD.md`](../sdd/EMPREGANET-SDD.md) |

---

## Depois da spec aprovada

Implementação e convenções: [`../skills/backend-skill/SKILL.md`](../skills/backend-skill/SKILL.md), [`../skills/frontend-skill/SKILL.md`](../skills/frontend-skill/SKILL.md); agentes em [`../agents/`](../agents/).

---

## Início rápido (copy-paste)

Ver blocos prontos em [`../sdd/SDD-USAGE-GUIDE.md`](../sdd/SDD-USAGE-GUIDE.md) (uma feature ou lote pequeno). Frase mínima:

> Orquestrador SDD EmpregaNet: criar `docs/features/<feature-id>/` e o `prd.md` (v1.0.0) com o problema de negócio e critérios de aceite — sem detalhe técnico.

As subpastas `<feature-id>/` aparecem à medida que as features forem especificadas; não é necessário listá-las neste README se já estiverem no vosso board ou em `FEATURES-BACKLOG.md`.
