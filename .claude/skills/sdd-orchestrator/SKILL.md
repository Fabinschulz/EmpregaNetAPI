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
| [`harness-contract`](../harness-contract/SKILL.md) | Contrato de circulação: fonte de verdade, confiança, Working Context, orçamento de contexto |

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
7. **YAGNI por fase** — o escopo corta-se aqui, onde é mais barato, antes de existir código: requisito sem utilizador
   e sem critério de aceite verificável fica fora do `prd.md`; mecanismo sem requisito do PRD que o obrigue não é
   desenhado no `design.md`; o que for adiado entra no `tasks.md` como **Adiado**, com gatilho de retorno.
   Excepção: os custos assimétricos — contrato HTTP, migration destrutiva, captura de dados, autorização — decidem-se
   agora, com fundamento. Critério completo na secção "YAGNI — o que não se constrói agora" da
   [`backend-skill`](../backend-skill/SKILL.md) e da [`frontend-skill`](../frontend-skill/SKILL.md).

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

O prompt de delegação leva o **Working Context** da `harness-contract` — com os `ACCEPTANCE_CRITERIA` vindos do
`spec.md` — e não o histórico das fases 1 a 3. O PRD e as discussões de desenho já estão condensados nos
artefactos aprovados; reenviá-los é pagar duas vezes pela mesma decisão.

### 4.1 Verificação contra a matriz de aceite

A tarefa **não** fecha por um agente dizer que terminou. Cada linha da matriz *critério de aceite → local de
verificação* do `spec.md` recebe um estado, com evidência:

| Estado | Significado |
| ------ | ----------- |
| **Verificado** | Comando corrido, teste verde, ou cenário navegado — com a evidência anexa |
| **Não verificado** | Implementado, mas sem prova nesta execução — dizer o que falta correr |
| **Não implementado** | Fora do que foi entregue — vai para `tasks.md` com o motivo |

Regras:

1. Nenhuma linha fica sem estado. Omissão silenciosa é a forma mais comum de dar por concluída uma feature incompleta.
2. Quem verifica não é quem implementou: `code-reviewer` para o diff, `test-engineer` para a rede de regressão,
   `e2e-qa-skill` para o comportamento pela UI.
3. Uma linha **Não verificado** impede confiança `HIGH` no fecho da feature — a entrega sai com a incerteza nomeada.
4. Divergência entre o implementado e o `design.md` é **achado**, não ajuste silencioso: ou o código corrige-se,
   ou o `design.md` sobe de versão (§3.2) com a decisão registada.

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
7. [ ] Capacidade conscientemente adiada está registada como **Adiado**, com gatilho de retorno (§3 regra 7).

---

## 7. Idioma

Artefactos e comunicação em **português (Brasil)**; identificadores técnicos e `feature-id` em inglês/kebab-case.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 2.2.0 | Fase 4 passa a delegar com o Working Context da `harness-contract` em vez do histórico das fases anteriores, e ganha §4.1: verificação linha a linha da matriz de aceite do `spec.md`, com estado e evidência por critério, verificada por agente diferente de quem implementou |
| 2.1.0 | Regra de YAGNI por fase (§3 regra 7) e item de validação para capacidade adiada, apoiados na secção "YAGNI — o que não se constrói agora" das skills de backend e frontend |
| 2.0.0 | Deixa de ser lista de ponteiros: passa a executor com entradas obrigatórias, tabela de fases com gates, gate de código accionável, regra de version bump, mapa de delegação e checklist de validação por fase. Movida para `.claude/skills/` |
| 1.0.0 | Redirecção para os documentos SDD |
