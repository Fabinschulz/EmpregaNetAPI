# Documentação EmpregaNet (`docs/`)

Índice **canónico** para IA e equipa alinharem ao produto neste monorepo. O contexto sempre aplicável no Cursor está em [`.cursor/rules/empreganet-docs-context.mdc`](../.cursor/rules/empreganet-docs-context.mdc).

---

## Mapa do repositório (raiz Git)

Confirma estes paths antes de assumir outros layouts.

| Pasta | Conteúdo típico |
| ----- | ----------------- |
| `backend/` | API principal .NET (**Clean Architecture**): `EmpregaNet.Domain`, `EmpregaNet.Application`, `EmpregaNet.Infra`, `EmpregaNet.Api`. Solução: `backend/EmpregaNet.sln`. Testes: `backend/tests/tests.csproj` (xUnit, FluentAssertions, Moq). |
| `Bff/` | Backend-for-Frontend .NET (`EmpregaNet.Bff.sln`). |
| `frontend/` | Next.js (App Router), TypeScript, SCSS — `package.json` usa **pnpm**. |
| `docs/` | SDD, agentes, skills, especificações por feature. |

Dominó técnico comum neste codebase: PostgreSQL via EF Core, cache Redis opcional (config `Redis`). O diagrama alto nível está em [`sdd/EMPREGANET-SDD.md`](sdd/EMPREGANET-SDD.md).

---

## Onde ler primeiro

| Prioridade | Documento |
| ---------- | --------- |
| 1 | [`sdd/EMPREGANET-SDD.md`](sdd/EMPREGANET-SDD.md) — filosofia, fases A–E, gates, IA |
| 2 | [`skills/backend-skill/SKILL.md`](skills/backend-skill/SKILL.md) e [`skills/frontend-skill/SKILL.md`](skills/frontend-skill/SKILL.md) — convenções de implementação |
| 3 | [`agents/meta-agent.md`](agents/meta-agent.md) — quando o pedido for largo |

---

## SDD e especificações por feature

| Documento | Quando usar |
| --------- | ----------- |
| [`sdd/SDD-ORCHESTRATOR.md`](sdd/SDD-ORCHESTRATOR.md) | Fluxo PRD → design → spec/tasks; gate antes de código |
| [`sdd/SDD-USAGE-GUIDE.md`](sdd/SDD-USAGE-GUIDE.md) | Templates de prompt, versões em frontmatter, `state.md` |
| [`skills/sdd-orchestrator/SKILL.md`](skills/sdd-orchestrator/SKILL.md) | Atalho Cursor para esse fluxo |
| [`features/README.md`](features/README.md) | Convenção `docs/features/<feature-id>/` |
| [`sdd/FEATURES-BACKLOG.md`](sdd/FEATURES-BACKLOG.md) | Índice opcional no Git (issue tracker pode ser a fonte de verdade) |
| [`sdd/adrs/README.md`](sdd/adrs/README.md) | ADRs transversais |

---

## Agentes (`agents/`)

| Ficheiro | Uso rápido |
| -------- | ---------- |
| `meta-agent.md` | Orquestração ou pedidos vagos |
| `dotnet-architect.md` | Fronteiras backend, layering |
| `dotnet-implementer.md` | Código .NET concreto |
| `frontend-engineer.md` | Next.js / React |
| `test-engineer.md` | Estratégia e qualidade de testes |
| `code-reviewer.md` | Diff / pré-merge |
| `debug-specialist.md` | Causa raiz |
| `performance-optimizer.md` | Performance com evidência |

---

## Comandos úteis (verificação local)

Na raiz do repositório (ajusta se o CI usar outra ordem):

```bash
dotnet build backend/EmpregaNet.sln
dotnet build Bff/EmpregaNet.Bff.sln
dotnet test backend/tests/tests.csproj
cd frontend && pnpm lint && pnpm build
```

Sem secrets no repo: usar apenas `.env.example` e variáveis de ambiente reais localmente ou no pipeline.

---

*Este README deve manter-se alinhado à estrutura real do código; atualiza ao mudar layouts de solução ou pastas principais.*
