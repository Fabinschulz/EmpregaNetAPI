---
name: sdd-orchestrator
description: Conduz o fluxo Spec-Driven Development do EmpregaNet fase a fase — prd.md, design.md, spec.md, tasks.md em docs/features/<feature-id>/ — com versionamento em frontmatter e gate que impede gerar código até aprovação humana de cada fase. Use ao iniciar uma capability nova, um refactor grande cujo contrato de negócio/técnico deva ficar documentado, ou quando o utilizador pedir "orquestrador SDD", "PRD primeiro" ou "spec antes de implementar". Não use para correcção de bug, ajuste pequeno dentro de contrato existente, nem para implementar uma feature cuja spec já está aprovada.
---

# Orquestrador SDD — EmpregaNet

Executor do fluxo SDD. O **governo** (fases, regras, mapeamento A–E) está em
[`docs/sdd/SDD-ORCHESTRATOR.md`](../../../docs/sdd/SDD-ORCHESTRATOR.md) — documento aprovado e versionado,
que esta skill **aplica** em vez de reescrever.

| Documento | Papel |
| --------- | ----- |
| [`docs/sdd/SDD-ORCHESTRATOR.md`](../../../docs/sdd/SDD-ORCHESTRATOR.md) | Regras de governo, conteúdo mínimo de cada artefacto, gate de código |
| [`docs/sdd/SDD-USAGE-GUIDE.md`](../../../docs/sdd/SDD-USAGE-GUIDE.md) | Templates de acionamento, *version bump*, geração de `state.md` |
| [`docs/sdd/EMPREGANET-SDD.md`](../../../docs/sdd/EMPREGANET-SDD.md) | Filosofia e fases A–E do produto |
| [`docs/sdd/adrs/`](../../../docs/sdd/adrs/) | Decisões estruturais duradouras |

Ler o primeiro **antes** de gerar qualquer artefacto.

---

## 1. Quando aplicar

| Situação | Aplicar |
| -------- | ------- |
| Capability nova com contrato de negócio a fixar | Sim |
| Refactor que muda fronteiras ou contratos entre camadas/serviços | Sim |
| Pedido explícito: "orquestrador SDD", "PRD primeiro", "spec antes de código" | Sim |
| Correcção de bug ou ajuste dentro de contrato existente | Não — agent `debug-specialist` ou implementação directa |
| Spec já aprovada, falta construir | Não — ir para a fase 4 (delegação) |
| Decisão técnica isolada e duradoura, sem feature associada | Não — escrever um ADR em `docs/sdd/adrs/` |

---

## 2. Entradas necessárias

Antes da Fase 1, exigir do utilizador (perguntar o que faltar — não inventar):

- **`feature-id`** em kebab-case (ex.: `emp-12-notificacao-candidatura`).
- **Problema de negócio** que a feature resolve.
- **Regras principais**: RBAC, invariantes, limites conhecidos.

Sem `feature-id` e sem problema de negócio, **não** criar pasta nem ficheiro.

---

## 3. Processo — uma fase por vez, com gate humano entre cada

| Fase | Artefacto | Gate de saída |
| ---- | --------- | ------------- |
| 1 | `prd.md` — problema, personas e RBAC, workflows, critérios de aceite verificáveis, non-goals | Aprovação humana do PRD |
| 2 | `design.md` — contratos de dados, fluxos (mermaid quando ajudar), HTTP (rotas/verbos/corpos/códigos), infra e políticas de auth | Aprovação humana do design |
| 3 | `spec.md` (matriz critério de aceite → local de verificação) + `tasks.md` (plano de implementação, com *deviation notes*) | Aprovação humana da spec |
| 4 | Implementação delegada | — |
| 5 | `state.md` (opcional) — congelamento pós-aprovação | — |

Regras de execução em cada fase:

1. Gerar **só** o artefacto da fase corrente e **parar** para aprovação. Nunca produzir dois artefactos numa passagem.
2. Todo artefacto nasce em **v1.0.0** com frontmatter `version` / `date` / `status: Draft | Approved`.
3. **Separação PRD vs design:** no `prd.md` não entram soluções técnicas, pacotes NuGet, EF, Redis, mensagens ou estruturas de BD.
4. **Sem duplicação entre artefactos:** o `spec.md` não repete endpoints nem tabelas (isso é `design.md`) e não dilui passos de implementação (isso é `tasks.md`).
5. **Dependências reais:** não inventar integrações ou assinaturas externas — exigir confirmação humana ou código existente no repositório.
6. **Simetria de domínio:** operações reversíveis (cancelar, despublicar, remover) modeladas com o mesmo rigor que as construtivas.

### 3.1 Gate de código (regra inviolável)

**Não** gerar, refactorizar ou alterar código de produção ou de testes de implementação antes de as fases acordadas estarem **explicitamente aprovadas** pelo humano. Excepção única: *spike* descartável pedido explicitamente, fora do PR da feature.

Se o utilizador pedir código com a spec ainda em Draft: dizer em uma frase que o gate está fechado, mostrar o que falta aprovar, e oferecer o *spike* descartável como alternativa.

### 3.2 Version bump

Ao alterar um artefacto já aprovado, subir a versão no frontmatter e actualizar a data:

| Bump | Quando |
| ---- | ------ |
| **Minor** (v1.1.0) | Novos critérios de aceite, campos ou endpoints compatíveis com o existente |
| **Major** (v2.0.0) | Mudança arquitectural, troca de integração crítica, ou regras que invalidam o contrato anterior |

---

## 4. Fase 4 — Delegação depois da spec aprovada

| Âmbito | Delegar a | Conhecimento que o agent carrega |
| ------ | --------- | ------------------------------- |
| Fronteiras, layering, forma da API | agent [`dotnet-architect`](../../agents/dotnet-architect.md) | [`backend-skill`](../backend-skill/SKILL.md) |
| Código .NET | agent [`dotnet-implementer`](../../agents/dotnet-implementer.md) | [`backend-skill`](../backend-skill/SKILL.md) |
| UI Next.js | agent [`frontend-engineer`](../../agents/frontend-engineer.md) | [`frontend-skill`](../frontend-skill/SKILL.md) |
| Testes | agent [`test-engineer`](../../agents/test-engineer.md) | ambas, conforme a camada |
| Verificação pela UI real | skill [`e2e-qa-skill`](../e2e-qa-skill/SKILL.md) | — |

Passar ao agent o caminho de `docs/features/<id>/design.md` e `tasks.md` — não recopiar o conteúdo no prompt.
Decisão estrutural que sobreviva à feature: registar um **ADR** em `docs/sdd/adrs/`.

---

## 5. Estrutura canónica

```text
docs/features/
└── <feature-id>/
    ├── prd.md
    ├── design.md
    ├── spec.md
    ├── tasks.md
    └── state.md            # opcional, após freeze
```

Uma pasta por feature. Não misturar features no mesmo ficheiro nem na raiz de `docs/`.

---

## 6. Validação antes de declarar uma fase concluída

1. [ ] O artefacto tem frontmatter com `version`, `date`, `status`.
2. [ ] Está na pasta `docs/features/<feature-id>/` correcta.
3. [ ] Não invade o âmbito de outra fase (§3 regras 3 e 4).
4. [ ] Critérios de aceite são **verificáveis** (observáveis, não aspiracionais).
5. [ ] Non-goals declarados no `prd.md`.
6. [ ] Nenhum código de produção foi tocado (§3.1).

---

## 7. Idioma

Artefactos e comunicação em **português (Brasil)**; identificadores técnicos e `feature-id` em inglês/kebab-case.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 2.0.0 | Deixa de ser lista de ponteiros: passa a executor com entradas obrigatórias, tabela de fases com gates, gate de código accionável, regra de version bump, mapa de delegação e checklist de validação por fase. Movida para `.claude/skills/` |
| 1.0.0 | Redirecção para os documentos SDD |
