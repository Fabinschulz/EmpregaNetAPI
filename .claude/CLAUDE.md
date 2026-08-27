# EmpregaNet - Contexto de desenvolvimento

Antes de implementar ou desenhar mudanças significativas, alinha-te ao **Spec-Driven Development** e à arquitectura descrita no repositório.

## Monorepo

| Pasta | Tecnologia |
|-------|-----------|
| `backend/` | .NET 10, Clean Architecture (Domain / Application / Infra / Api) |
| `Bff/` | .NET 10, BFF (Core / Infrastructure / WebApi) |
| `frontend/` | Next.js 16, React 19, TypeScript strict, SCSS + Radix/ShadCN, pnpm |

Mapa completo de pastas e comandos de build: [`docs/README.md`](../docs/README.md)

## Fonte principal - SDD

- **Especificação do produto:** [`docs/sdd/EMPREGANET-SDD.md`](../docs/sdd/EMPREGANET-SDD.md) - princípios, camadas, fases A–E, gates de verificação.
- **Fluxo por feature:** [`docs/sdd/SDD-ORCHESTRATOR.md`](../docs/sdd/SDD-ORCHESTRATOR.md) e [`docs/sdd/SDD-USAGE-GUIDE.md`](../docs/sdd/SDD-USAGE-GUIDE.md). Artefactos em `docs/features/<feature-id>/`.
- **ADRs:** [`docs/sdd/adrs/`](../docs/sdd/adrs/) - decisões estruturais duradouras.
- **Backlog:** [`docs/sdd/FEATURES-BACKLOG.md`](../docs/sdd/FEATURES-BACKLOG.md) (quando existir).

## Agentes especialistas (`.claude/agents/`)

Invoca pelo **nome** com a ferramenta Agent (`subagent_type`). Cada agente já traz a sua allowlist de
ferramentas e lê a skill correspondente no arranque — não copies convenções para o prompt de delegação.
Índice e padrão de escrita: [`docs/agents/README.md`](../docs/agents/README.md).

| Situação | Agente |
|----------|--------|
| Fronteiras / layering / API shape (read-only) | `dotnet-architect` |
| Implementação .NET concreta | `dotnet-implementer` |
| UI / Next.js / React | `frontend-engineer` |
| Testes automatizados | `test-engineer` |
| Qualidade de PR / diff (read-only) | `code-reviewer` |
| Bugs / causa raiz | `debug-specialist` |
| Performance com evidência | `performance-optimizer` |
| QA End-to-End (navega a UI real) | `e2e-qa-engineer` |

Orquestração mínima: um especialista quando bastar; cadeias curtas só quando a tarefa exigir.

## Skills (`.claude/skills/`)

Carregadas automaticamente quando a situação encaixa, ou por `/<nome>`. Índice e padrão:
[`docs/skills/README.md`](../docs/skills/README.md).

| Área | Skill |
|------|-------|
| Convenções backend .NET (conhecimento) | `backend-skill` |
| Convenções frontend Next.js (conhecimento) | `frontend-skill` |
| Pedido vago ou multi-domínio → rotear e encadear | `/meta-agent` |
| Especificar feature antes de código (gate por fase) | `/sdd-orchestrator` |
| Regressão E2E pela UI real | `/e2e-qa-skill` |

Orquestração é skill, não agente: um subagente não tem a ferramenta Agent e por isso só conseguiria
recomendar, não delegar.

## Regras de comportamento

- **SDD first:** para features novas ou refactors com contrato negócio/técnico, seguir o fluxo SDD (PRD → design → spec/tasks) antes de gerar código.
- **Human-in-the-loop:** merge e decisões de risco ficam com o humano. Sem secrets no repo.
- **Segurança:** autorização (RBAC) explícita onde o SDD e a feature exigirem; validar inputs na fronteira.
- **Clean Architecture:** dependências apontam para dentro (Domain ← Application ← Infra/Api). No Domain são proibidos `Microsoft.EntityFrameworkCore` (DbContext, DbSet, migrations), `Microsoft.AspNetCore.Mvc` e tipos de HTTP. **Exceção registada:** `User`/`Role` herdam do ASP.NET Core Identity - ver [ADR 0005](../docs/sdd/adrs/0005-identity-no-dominio.md).
- **Mediator interno:** usar `IRequest` / `IRequestHandler` de `EmpregaNet.Domain.Libs.Mediator`; não introduzir MediatR nem outro barramento sem alinhamento.
- **Frontend:** TypeScript `strict`, sem `any`, SCSS - não expandir Tailwind.
- **Idioma:** respostas e artefactos em **português (Brasil)**; identificadores de código em inglês.
