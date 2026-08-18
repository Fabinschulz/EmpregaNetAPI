# Documentação EmpregaNet (`docs/`)

Índice **canónico** para IA e equipe alinharem ao produto neste monorepo. O contexto sempre aplicável está em [`../CLAUDE.md`](../CLAUDE.md), na raiz do repositório.

> O guia de onboarding completo (arquitectura, diagramas, fluxos, setup, convenções) é o [`README.md`](../README.md) da raiz. Esta pasta cobre **processo** (SDD, ADRs, agentes, skills).

---

## Mapa do repositório (raiz Git)

Confirma estes paths antes de assumir outros layouts.

| Pasta | Conteúdo típico |
| ----- | ----------------- |
| `backend/` | API principal .NET (**Clean Architecture**): `EmpregaNet.Domain`, `EmpregaNet.Application`, `EmpregaNet.Infra`, `EmpregaNet.Api`. Solução: `backend/EmpregaNet.sln`. Testes: `backend/tests/tests.csproj` (xUnit, FluentAssertions, Moq). |
| `Bff/` | Backend-for-Frontend .NET (`EmpregaNet.Bff.sln`). |
| `frontend/` | Next.js (App Router), TypeScript, SCSS - `package.json` usa **pnpm**. |
| `docs/` | SDD, agentes, skills, especificações por feature. |

Dominó técnico comum neste codebase: PostgreSQL via EF Core, cache Redis opcional (config `Redis`). O diagrama alto nível está em [`sdd/EMPREGANET-SDD.md`](sdd/EMPREGANET-SDD.md).

---

## Onde ler primeiro

| Prioridade | Documento |
| ---------- | --------- |
| 1 | [`sdd/EMPREGANET-SDD.md`](sdd/EMPREGANET-SDD.md) - filosofia, fases A–E, gates, IA |
| 2 | [`skills/backend-skill/SKILL.md`](skills/backend-skill/SKILL.md) e [`skills/frontend-skill/SKILL.md`](skills/frontend-skill/SKILL.md) - convenções de implementação |
| 3 | [`agents/meta-agent.md`](agents/meta-agent.md) - quando o pedido for largo |

---

## SDD e especificações por feature

| Documento | Quando usar |
| --------- | ----------- |
| [`sdd/SDD-ORCHESTRATOR.md`](sdd/SDD-ORCHESTRATOR.md) | Fluxo PRD → design → spec/tasks; gate antes de código |
| [`sdd/SDD-USAGE-GUIDE.md`](sdd/SDD-USAGE-GUIDE.md) | Templates de prompt, versões em frontmatter, `state.md` |
| [`skills/sdd-orchestrator/SKILL.md`](skills/sdd-orchestrator/SKILL.md) | Atalho de IA para esse fluxo |
| [`sdd/adrs/README.md`](sdd/adrs/README.md) | ADRs transversais (índice dos 7 existentes) |

`docs/features/<feature-id>/` é a convenção para specs por feature (`prd.md`, `design.md`, `spec.md`, `tasks.md`). Um `sdd/FEATURES-BACKLOG.md` é opcional: o issue tracker pode ser a fonte de verdade.

| Feature | Escopo |
| ------- | ------ |
| [`features/emp-feed-vagas/`](features/emp-feed-vagas/prd.md) | Feed público de vagas: agregado `Job` enriquecido, busca full-text, filtros combináveis na URL e scroll infinito |

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
| `e2e-qa-engineer.md` | Regressão E2E navegando a UI real (Browser tool) |

---

## Skills (`skills/`)

| Ficheiro | Uso rápido |
| -------- | ---------- |
| `backend-skill/SKILL.md` | Convenções .NET |
| `frontend-skill/SKILL.md` | Convenções Next.js/React |
| `sdd-orchestrator/SKILL.md` | Fluxo PRD → design → spec/tasks |
| `e2e-qa-skill/SKILL.md` | Metodologia de regressão E2E via Browser |

Equivalente no Claude Code deste repositório (invocável nativamente via subagent `e2e-qa-engineer` e skill `/e2e-qa-skill`): [`.claude/agents/e2e-qa-engineer.md`](../.claude/agents/e2e-qa-engineer.md) e [`.claude/skills/e2e-qa-skill/SKILL.md`](../.claude/skills/e2e-qa-skill/SKILL.md) — apontam para os arquivos acima como fonte canônica da metodologia; mantenha os dois em sync se atualizar um.

---

## Comandos úteis (verificação local)

Na raiz do repositório (ajusta se o CI usar outra ordem):

```bash
dotnet build backend/EmpregaNet.sln
dotnet build Bff/EmpregaNet.Bff.sln
dotnet test backend/tests/tests.csproj
cd frontend && pnpm lint && pnpm test && pnpm build
```

Sem secrets no repo. Os templates versionados são `backend/src/EmpregaNet.Api/appsettings.example.json`, `backend/.env.example`, `Bff/.env.example` e `frontend/.env.example` - copie-os e preencha localmente; em produção, use variáveis de ambiente.

---

*Este README deve manter-se alinhado à estrutura real do código; atualiza ao mudar layouts de solução ou pastas principais.*
