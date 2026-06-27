# EmpregaNet — Contexto de desenvolvimento

Antes de implementar ou desenhar mudanças significativas, alinha-te ao **Spec-Driven Development** e à arquitectura descrita no repositório.

## Monorepo

| Pasta | Tecnologia |
|-------|-----------|
| `backend/` | .NET 10, Clean Architecture (Domain / Application / Infra / Api) |
| `Bff/` | .NET 10, BFF (Core / Infrastructure / WebApi) |
| `frontend/` | Next.js 16, React 19, TypeScript strict, SCSS + Radix/ShadCN, pnpm |

Mapa completo de pastas e comandos de build: [`docs/README.md`](docs/README.md)

## Fonte principal — SDD

- **Especificação do produto:** [`docs/sdd/EMPREGANET-SDD.md`](docs/sdd/EMPREGANET-SDD.md) — princípios, camadas, fases A–E, gates de verificação.
- **Fluxo por feature:** [`docs/sdd/SDD-ORCHESTRATOR.md`](docs/sdd/SDD-ORCHESTRATOR.md) e [`docs/sdd/SDD-USAGE-GUIDE.md`](docs/sdd/SDD-USAGE-GUIDE.md). Artefactos em `docs/features/<feature-id>/`.
- **ADRs:** [`docs/sdd/adrs/`](docs/sdd/adrs/) — decisões estruturais duradouras.
- **Backlog:** [`docs/sdd/FEATURES-BACKLOG.md`](docs/sdd/FEATURES-BACKLOG.md) (quando existir).

## Agentes especialistas (`docs/agents/`)

Antes de delegar trabalho profundo, lê o perfil do agente correspondente.  
Usa a ferramenta **Agent** (subagent_type `claude`) com o conteúdo do arquivo como prompt de sistema.

| Situação | Agente |
|----------|--------|
| Pedido vago ou multi-domínio | [`meta-agent.md`](docs/agents/meta-agent.md) |
| Fronteiras / layering / API shape | [`dotnet-architect.md`](docs/agents/dotnet-architect.md) |
| Implementação .NET concreta | [`dotnet-implementer.md`](docs/agents/dotnet-implementer.md) |
| UI / Next.js / React | [`frontend-engineer.md`](docs/agents/frontend-engineer.md) |
| Qualidade de PR / diff | [`code-reviewer.md`](docs/agents/code-reviewer.md) |
| Testes | [`test-engineer.md`](docs/agents/test-engineer.md) |
| Bugs / causa raiz | [`debug-specialist.md`](docs/agents/debug-specialist.md) |
| Performance | [`performance-optimizer.md`](docs/agents/performance-optimizer.md) |

Orquestração mínima: um especialista quando bastar; cadeias curtas só quando a tarefa exigir (ver `meta-agent.md`).

## Skills de implementação (`docs/skills/`)

| Área | Skill |
|------|-------|
| Backend .NET | [`docs/skills/backend-skill/SKILL.md`](docs/skills/backend-skill/SKILL.md) |
| Frontend Next.js | [`docs/skills/frontend-skill/SKILL.md`](docs/skills/frontend-skill/SKILL.md) |
| Orquestrador SDD | [`docs/skills/sdd-orchestrator/SKILL.md`](docs/skills/sdd-orchestrator/SKILL.md) |

## Regras de comportamento

- **SDD first:** para features novas ou refactors com contrato negócio/técnico, seguir o fluxo SDD (PRD → design → spec/tasks) antes de gerar código.
- **Human-in-the-loop:** merge e decisões de risco ficam com o humano. Sem secrets no repo.
- **Segurança:** autorização (RBAC) explícita onde o SDD e a feature exigirem; validar inputs na fronteira.
- **Clean Architecture:** dependências apontam para dentro (Domain ← Application ← Infra/Api). Domain não referencia EF nem ASP.NET.
- **Mediator interno:** usar `IRequest` / `IRequestHandler` de `EmpregaNet.Domain.Libs.Mediator`; não introduzir MediatR nem outro barramento sem alinhamento.
- **Frontend:** TypeScript `strict`, sem `any`, SCSS — não expandir Tailwind.
- **Idioma:** respostas e artefactos em **português (Brasil)**; identificadores de código em inglês.
