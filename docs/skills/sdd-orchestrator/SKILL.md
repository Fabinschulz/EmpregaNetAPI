---
name: sdd-orchestrator
description: >-
  Especifica features pelo método SDD EmpregaNet: pastas em docs/features/, artefactos prd/design/spec/tasks,
  versionamento em frontmatter, gate anti-código até aprovação. Use ao iniciar uma nova capability, refactor grande
  com spec formal, ou quando o utilizador pede fluxo "orquestrador SDD".
---

# Skill — Orquestrador SDD (EmpregaNet)

## Quando aplicar

- Nova funcionalidade com especificação antes de código.
- Refactor ou novo módulo em que o contrato negócio/técnico deve ficar documentado por fases.
- Pedido explícito: "orquestrador SDD", "PRD primeiro", "spec antes de implementar".

## Documentos de referência (ordem)

1. [`docs/sdd/SDD-ORCHESTRATOR.md`](../../sdd/SDD-ORCHESTRATOR.md) — regras, fases, gate de código, estrutura de pastas.
2. [`docs/sdd/SDD-USAGE-GUIDE.md`](../../sdd/SDD-USAGE-GUIDE.md) — templates de comando, *version bump*, geração de `state.md`.
3. [`docs/sdd/EMPREGANET-SDD.md`](../../sdd/EMPREGANET-SDD.md) — alinhamento com fases A–E e princípios do repositório.

## Após a spec aprovada

- Backend: [`docs/skills/backend-skill/SKILL.md`](../backend-skill/SKILL.md) + agentes `dotnet-architect` / `dotnet-implementer` conforme necessidade.
- Frontend: [`docs/skills/frontend-skill/SKILL.md`](../frontend-skill/SKILL.md) + `frontend-engineer`.
- ADRs transversais: `docs/sdd/adrs/`.

## Idioma

Texto e artefactos em **português**; nomes técnicos e código em inglês quando for convenção do repo.
