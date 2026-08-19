# Especificações por feature — índice

Uma pasta por feature, com os artefactos do fluxo SDD. O processo está em
[`../sdd/SDD-ORCHESTRATOR.md`](../sdd/SDD-ORCHESTRATOR.md); a execução, na skill
[`sdd-orchestrator`](../../.claude/skills/sdd-orchestrator/SKILL.md).

```text
docs/features/
└── <feature-id>/           # kebab-case, ex.: emp-12-notificacao-candidatura
    ├── prd.md              # negócio, personas/RBAC, critérios de aceite, non-goals
    ├── design.md           # contratos, fluxos, HTTP, infra
    ├── spec.md             # matriz critério de aceite → local de verificação
    ├── tasks.md            # plano de implementação + deviation notes
    └── state.md            # opcional, após freeze
```

Todo artefacto nasce em **v1.0.0** com frontmatter `version` / `date` / `status: Draft | Approved`.
Regras de *version bump*: [`../sdd/SDD-USAGE-GUIDE.md`](../sdd/SDD-USAGE-GUIDE.md).

---

## Features

| Feature | Escopo | Estado |
| ------- | ------ | ------ |
| [`emp-feed-vagas/`](emp-feed-vagas/prd.md) | Feed público de vagas: agregado `Job` enriquecido, busca full-text, filtros combináveis na URL, scroll infinito | `prd` · `design` · `spec` · `tasks` |

Prioridades de alto nível ficam no issue tracker. Um índice textual opcional pode viver em
`../sdd/FEATURES-BACKLOG.md`, sem substituir as issues.
