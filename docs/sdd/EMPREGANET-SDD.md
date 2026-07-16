# EmpregaNet — Spec-Driven Development (SDD)

Documento vivo: descreve **como** especificação, implementação e validação se alinham no monorepo **EmpregaNet**, com ênfase em contratos claros, gates de qualidade e uso disciplinado de **agentes de IA** como aceleradores—not substitutos de julgamento humano.

**Audiência**: equipe de produto e engenharia, arquitetos, e quem orquestra trabalho com IA (Cursor, reviews automatizados, etc.).

---

## 1. Filosofia e princípios

### 1.1 O que é SDD neste repositório

**Spec-Driven Development**: toda entrega significativa começa com **artefactos de especificação verificáveis** (comportamento esperado, contratos, critérios de aceitação), prossegue com **implementação rastreável** à spec, e fecha com **evidência** (testes, revisão, métricas quando aplicável). Não é documentação ornamental: a spec **orienta** o código e **detecta deriva**.

### 1.2 Princípios (arquitectura + IA)

| Princípio | Implicação prática |
| --------- | ------------------- |
| **Single source of truth** | Comportamento de negócio: linguagem ubíqua + testes/regras na Application (.NET). Contrato HTTP: ViewModels/DTOs estáveis; no frontend, **Zod** espelha o contrato consumido. |
| **Human-in-the-loop** | IA (agentes em `docs/agents/`) acelera redacção, implementação e revisão; **merge** e decisões de risco ficam com humanos. |
| **Least power** | Preferir o mecanismo mais simples que satisfaz a spec (YAGNI, KISS, DRY). |
| **Traceability** | Cada PR liga-se a uma spec ou ticket com critérios de aceitação mensuráveis. |
| **Defence in depth** | Validação na API, saneamento na fronteira do cliente, autorização explícita (RBAC) em caminhos sensíveis. |

### 1.3 Visão do sistema (C4 model — nível contentor)

```mermaid
flowchart LR
  subgraph clients [Clientes]
    WEB[Next.js frontend]
  end
  subgraph edge [Orquestração]
    BFF[BFF .NET]
  end
  subgraph core [Core]
    API[EmpregaNet API .NET]
    DB[(PostgreSQL)]
    CACHE[(Redis / cache)]
  end
  WEB --> BFF
  BFF --> API
  API --> DB
  API --> CACHE
```

- **`frontend/`**: experiência utilizador, sessão/cookies, RBAC de UI, chamadas ao BFF ou API conforme configuração (Next.js na raiz; ver `frontend/package.json` — gestor **pnpm**).
- **`Bff/`**: projeto .NET de agregação/orquestração HTTP para o cliente (`Bff/EmpregaNet.Bff.sln`).
- **`backend/`**: domínio, casos de uso, persistência (EF Core, PostgreSQL), cache Redis opcional, autenticação/autorização da API (`backend/EmpregaNet.sln`; testes em `backend/tests/`).

### 1.4 Mapa de pastas (contexto efectivo)

Referência rápida alinhada ao **estado actual** do repo; detalhes e comandos em [`../README.md`](../README.md).

| Local | Função principal |
| ----- | ----------------- |
| `backend/src/EmpregaNet.*` | Camadas Domain, Application, Infra, Api (.NET — target conforme `.csproj` do projeto) |
| `backend/tests/` | Testes automatizados (xUnit + FluentAssertions + Moq, integração com fixtures partilhadas quando aplicável) |
| `Bff/` | BFF .NET com solução dedicada |
| `frontend/` | UI Next.js (App Router), TypeScript, SCSS, Zod, RBAC em UI |
| `docs/` | SDD, agentes, skills e `docs/features/<id>/` para specs opcionais |

---

## 2. Camadas de especificação

Ordem sugerida de riqueza vs custo de manutenção:

1. **Critérios de aceitação** (Gherkin (linguagem estruturada) opcional, bullets obrigatórios no ticket/PR): dado/quando/então em linguagem de negócio.
2. **Contrato de API** (OpenAPI/Swagger se gerado; caso contrário **documentação de endpoints** + exemplos JSON alinhados aos ViewModels).
3. **Schemas Zod** no frontend para payloads que o UI consome ou envia (paridade com a API).
4. **Testes automatizados**: unitários na Application; integração para EF/HTTP; e2e mínimos para fluxos críticos (login, candidatura, etc., conforme produto).

**Regra de ouro**: alterar contrato sem actualizar consumidor + spec + testes relevantes é **deriva intencional**, deve ser explícita no changelog do PR.

### 2.1 Especificação por feature (orquestrador — *best of both worlds*)

Para trabalho com **artefactos dedicados por pasta** (como em projectos SDD com `prd` / `design` / `spec` / `tasks`):

| Fase global (acima) | Artefactos em `docs/features/<feature-id>/` |
| ------------------- | --------------------------------------------- |
| A — Descoberta e especificação | `prd.md` (negócio e CA; **sem** pormenor técnico) |
| B — Desenho técnico | `design.md` (contratos, HTTP, diagramas, infra relevante) |
| Preparação da implementação | `spec.md` (matriz CA → cobertura), `tasks.md` (plano + *deviation notes*) |
| Congelamento opcional | `state.md` quando a spec estiver aprovada para desenvolvimento |

Regras operacionais, versionamento em frontmatter e **gate** “sem código até aprovação por fase”: [`SDD-ORCHESTRATOR.md`](SDD-ORCHESTRATOR.md). Templates de acionamento e geração de `state.md`: [`SDD-USAGE-GUIDE.md`](SDD-USAGE-GUIDE.md). Índice de pastas: [`../features/README.md`](../features/README.md). Skill de IA: [`../skills/sdd-orchestrator/SKILL.md`](../skills/sdd-orchestrator/SKILL.md).

---

## 3. Fluxo de trabalho SDD (operacional)

### Fase A — Descoberta e especificação

1. **Problema e não-objectivos**: o que não será feito neste incremento.
2. **Personas e permissões**: que papéis (RBAC) afectam a feature.
3. **Casos de uso** e **estados** (vazio, erro, loading, sucesso).
4. **Decisões de arquitectura leves**: se a decisão tiver trade-offs duradouros, registe **ADR** curto em `docs/sdd/adrs/` (template abaixo).

### Fase B — Desenho técnico (quando necessário)

- Backend: (Clean arch) Domain / Application / Infra / Api; comandos e queries com mediator interno do projecto.
- Frontend: pastas por feature, serviços e schemas por domínio.
- Orquestração IA: consultar `docs/agents/meta-agent.md` para pedidos amplos; `dotnet-architect` para novas fronteiras; `frontend-engineer` para UI.

### Fase C — Implementação

- Seguir `docs/skills/backend-skill/SKILL.md` e `docs/skills/frontend-skill/SKILL.md`.
- Commits pequenos, mensagens que referenciam o ticket/spec.

### Fase D — Verificação (gates)

| Gate | Mínimo esperado |
| --------- | ----------------- |
| **Build** | Na raiz do repo: `dotnet build backend/EmpregaNet.sln` e `dotnet build Bff/EmpregaNet.Bff.sln`; em `frontend/`: `pnpm lint` e `pnpm build`. |
| **Testes** | `dotnet test backend/tests/tests.csproj` verde quando aplicável; novos caminhos cobertos em Application ou integração. |
| **Segurança** | Sem secrets no repo; validação de input; autorização nos endpoints sensíveis. |
| **Revisão** | Humano + opcional passagem mental alinhada a [`docs/agents/code-reviewer.md`](../agents/code-reviewer.md). |

### Fase E — Entrega e observabilidade

- Métricas/logs para erros 5xx e latência em endpoints alterados.
- Feature flags apenas se o processo de release do projecto as usar.

---

## 4. ADR (Architecture Decision Record) — template

Criar ficheiro `docs/sdd/adrs/NNNN-titulo-curto.md`:

```markdown
# ADR NNNN: Título

## Status
Proposto | Aceite | Depreciado

## Contexto
Que forças e restrições levaram a esta decisão?

## Decisão
O que foi decidido (uma frase + bullets se necessário).

## Consequências
Positivas e negativas; o que fica proibido ou obrigatório daqui em diante.
```

---

## 5. Integração com agentes de IA (mapa mental)

Os prompts em `docs/agents/` são **perfis cognitivos** especializados. Use-os para reduzir erro de “generalista” em tarefas exigentes:

| Fase / necessidade | Agente sugerido |
| -------------------- | ----------------- |
| Especificação formal (PRD → tasks antes de código) | Fluxo [`SDD-ORCHESTRATOR.md`](SDD-ORCHESTRATOR.md) + opcionalmente `meta-agent` para encaminhar depois |
| PR ou diff | `code-reviewer` |
| Bug ou incidente | `debug-specialist` |
| Novo módulo API / fronteiras | `dotnet-architect` → `dotnet-implementer` |
| Código .NET concreto | `dotnet-implementer` |
| UI/UX | `frontend-engineer` |
| Testes | `test-engineer` |
| Performance com números ou suspeita forte | `performance-optimizer` |
| Pedido vago ou multi-domínio | `meta-agent` |

O ficheiro [`.cursor/rules/empreganet-docs-context.mdc`](../../.cursor/rules/empreganet-docs-context.mdc) garante que este SDD e as skills façam parte do contexto em sessões de edição.

Índice geral dos documentos desta pasta: [`../README.md`](../README.md).

---

## 6. IA como “co-arquitecto” (boas práticas)

1. **Context packing**: antes de pedir implementação longa, anexe paths relevantes, contrato JSON de exemplo, e critérios de aceitação.
2. **Verificação adversarial**: peça explicitamente ao `code-reviewer` o que falhou na primeira versão.
3. **Sem alucinação de stack**: o backend usa mediator **interno** em `EmpregaNet.Domain.Libs.Mediator`, não assumir MediatR NuGet sem verificar.
4. **Contratos duplos**: mudança na API → actualizar Zod/DTOs no cliente na mesma entrega quando possível.

---

## 7. Roadmap deste SDD

- Preencher `docs/sdd/adrs/` quando surgirem decisões estruturais (ex.: uso do BFF (`Bff/`) vs chamada directa ao `backend/` para algumas telas, política de cache).
- Usar `docs/features/<feature-id>/` para specs completas quando o fluxo orquestrador (`SDD-ORCHESTRATOR.md`) estiver em vigor para um incremento.
- Opcional: gerar ou anexar OpenAPI a partir da API para fonte única de contrato HTTP.
- Opcional: definir um conjunto mínimo de cenários e2e “golden path” alinhados ao negócio EmpregaNet.

---

## 8. Referências internas

| Recurso | Path |
| --------- | ------ |
| Índice `docs/` (mapa repo + comandos) | [`../README.md`](../README.md) |
| Regras Cursor (sempre aplicável) | [`.cursor/rules/empreganet-docs-context.mdc`](../../.cursor/rules/empreganet-docs-context.mdc) |
| Agentes | `docs/agents/*.md` |
| Skills | `docs/skills/*/SKILL.md` (backend, frontend, orquestrador SDD) |
| SDD (este documento) | `docs/sdd/EMPREGANET-SDD.md` |
| Orquestrador SDD (fases PRD→tasks) | `docs/sdd/SDD-ORCHESTRATOR.md` |
| Guia de uso (templates, `state.md`) | `docs/sdd/SDD-USAGE-GUIDE.md` |
| Pastas de feature | `docs/features/<feature-id>/` |
| ADRs | `docs/sdd/adrs/` |
| Backlog index (opcional) | `docs/sdd/FEATURES-BACKLOG.md` |

---

*Última revisão conceitual: monorepo EmpregaNet (`backend/` + `Bff/` + `frontend/`) — backend .NET Clean Architecture, mediator interno (`EmpregaNet.Domain.Libs.Mediator`), PostgreSQL + Redis opcional, frontend Next.js 16 + React 19 (pnpm).*
