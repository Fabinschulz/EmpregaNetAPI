# Documentação EmpregaNet (`docs/`)

Índice **canónico** para IA e equipe alinharem ao produto neste monorepo. O contexto sempre aplicável está em [`../.claude/CLAUDE.md`](../.claude/CLAUDE.md), na raiz do repositório.

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
| 2 | [`backend-skill`](../.claude/skills/backend-skill/SKILL.md) e [`frontend-skill`](../.claude/skills/frontend-skill/SKILL.md) - convenções de implementação |
| 3 | skill [`meta-agent`](../.claude/skills/meta-agent/SKILL.md) - quando o pedido for largo |

---

## SDD e especificações por feature

| Documento | Quando usar |
| --------- | ----------- |
| [`sdd/SDD-ORCHESTRATOR.md`](sdd/SDD-ORCHESTRATOR.md) | Fluxo PRD → design → spec/tasks; gate antes de código |
| [`sdd/SDD-USAGE-GUIDE.md`](sdd/SDD-USAGE-GUIDE.md) | Templates de prompt, versões em frontmatter, `state.md` |
| Skill [`sdd-orchestrator`](../.claude/skills/sdd-orchestrator/SKILL.md) | Executor do fluxo, com gate por fase |
| [`sdd/adrs/README.md`](sdd/adrs/README.md) | ADRs transversais (índice dos 9 existentes) |

`docs/features/<feature-id>/` é a convenção para specs por feature (`prd.md`, `design.md`, `spec.md`, `tasks.md`) - ver [`features/README.md`](features/README.md). Um `sdd/FEATURES-BACKLOG.md` é opcional: o issue tracker pode ser a fonte de verdade.

| Feature | Escopo |
| ------- | ------ |
| [`features/emp-feed-vagas/`](features/emp-feed-vagas/prd.md) | Feed público de vagas: agregado `Job` enriquecido, busca full-text, filtros combináveis na URL e scroll infinito |

---

## Agentes (`.claude/agents/`)

Invocáveis pelo **nome** via ferramenta Agent. Padrão de escrita e separação de responsabilidades:
[`agents/README.md`](agents/README.md).

| Agente | Uso rápido | Escreve código? |
| ------ | ---------- | --------------- |
| `dotnet-architect` | Fronteiras backend, layering, forma da API | Não — read-only |
| `dotnet-implementer` | Código .NET concreto, com build + testes | Sim |
| `frontend-engineer` | Next.js / React, com lint + testes + build | Sim |
| `test-engineer` | Testes automatizados (xUnit, Cucumber) | Só testes |
| `code-reviewer` | Diff / pré-merge | Não — read-only |
| `debug-specialist` | Causa raiz, correção mínima verificada | Sim |
| `performance-optimizer` | Performance com evidência medida | Sim |
| `e2e-qa-engineer` | Regressão E2E navegando a UI real (Browser) | Não altera código |

---

## Skills (`.claude/skills/`)

Carregadas automaticamente pela `description`, ou invocadas por `/<nome>`. Padrão:
[`skills/README.md`](skills/README.md).

| Skill | Tipo | Uso rápido |
| ----- | ---- | ---------- |
| `backend-skill` | Conhecimento | Convenções .NET: camadas, mediator interno, EF Core, contrato HTTP, testes |
| `frontend-skill` | Conhecimento | Convenções Next.js: `cacheComponents`, SCSS Modules, Zod, auth/RBAC, loading |
| `/meta-agent` | Orquestração | Roteia pedido vago ou multi-domínio para o especialista certo |
| `/sdd-orchestrator` | Orquestração | PRD → design → spec/tasks com gate humano por fase |
| `/e2e-qa-skill` | Orquestração + metodologia | Regressão E2E pela UI real |

Duas decisões estruturais que explicam este layout:

- **Orquestração é skill, não agente** — um subagente não tem acesso à ferramenta Agent, logo só conseguiria
  recomendar, não delegar. Skills correm na thread principal.
- **Conhecimento vive só nas skills** — nenhum agente recopia convenções do projeto. Cada agente declara
  `## Contexto obrigatório` e lê a skill no arranque (subagentes não têm a ferramenta Skill, mas têm `Read`).

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

## E-mail em desenvolvimento

Em **`Development`** e sem SMTP configurado (`Smtp:Enabled=false`, ou sem `Smtp:Host`/`Smtp:FromEmail`), a
API resolve `IEmailSender` para `DevelopmentLogEmailSender` (`backend/src/EmpregaNet.Infra/Email/`): nada é
entregue e a mensagem vai para o log com o prefixo `[E-MAIL DEV]`.

```jsonc
// backend/src/EmpregaNet.Api/appsettings.Development.json
"Smtp": { "Enabled": false }
```

Ou por variável de ambiente, sem tocar no ficheiro:

```bash
Smtp__Enabled=false dotnet run --project backend/src/EmpregaNet.Api
```

Dois níveis, de propósito:

| Nível | O que sai |
|-------|-----------|
| `Information` | Destinatário e assunto |
| `Debug` | Corpo HTML completo |

O corpo fica em `Debug` porque carrega **tokens vivos** de reset de senha e confirmação de conta: um link
registado é um link utilizável por quem leia o log. Para o inspecionar, baixe o nível localmente:

```bash
Logging__LogLevel__EmpregaNet.Infra.Email=Debug dotnet run --project backend/src/EmpregaNet.Api
```

Fora de `Development` este transporte **não** é usado: sem SMTP, os outros ambientes ficam com o no-op
silencioso, e `Production` nem sobe com `Smtp:Enabled=false`.

Serve para verificar pela UI real o conteúdo de e-mails transacionais (reset de senha, confirmação de
conta e as notificações de andamento de candidatura) sem servidor SMTP e sem caixa de correio.

---

*Este README deve manter-se alinhado à estrutura real do código; atualiza ao mudar layouts de solução ou pastas principais.*
