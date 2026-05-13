---
name: frontend-skill
description: >-
  Skill completa EmpregaNet Frontend: Next.js App Router, TypeScript estrito, SCSS + Radix/ShadCN sem Tailwind,
  Zod e React Hook Form, serviços por domínio, auth/RBAC, estados UX, SSE opcional, testes (Testing Library, E2E quando existir).
  Use ao criar ou alterar UI, hooks, cliente HTTP, fluxos ou acessibilidade no monorepo frontend.
author: EmpregaNet
version: 2.0.0
date: 2026-05-07
status: Approved
---

# Frontend (Next.js — monorepo EmpregaNet)

Documento **único** que combina disciplina tipo “skills de referência” (clean component, DIP, UX rigorosa,
estrategias de erro) com as **decisões fechadas** deste projeto (Tailwind não entra na stack atual).

---

## 1. Quando aplicar

| Situação | Usar esta skill |
| -------- | ---------------- |
| Todo o trabalho em `frontend/` (App Router), ou caminhos equivalentes no repo | Sim |
| Novas páginas, componentes, hooks, estilos, rotas middleware | Sim |
| Integração APIs + validação ao renderizar dados | Sim |
| Refactors que dividem componentes grandes ou melhoram a11y | Sim |

Este monorepo inclui **`frontend/`** na raiz do Git ao lado de **`backend/`** e **`Bff/`** — aplica esta skill sempre que editares esse código (ou clones do mesmo repo noutras máquinas).

---

## 2. Ligações obrigatórias

| Recurso | Path |
| ------- | ---- |
| Mapa `docs/` e estrutura do monorepo | [`docs/README.md`](../../README.md) |
| Agente de implementação frontend | [`docs/agents/frontend-engineer.md`](../../agents/frontend-engineer.md) |
| Fluxo especificação (quando há pasta `docs/features/`) | [`docs/sdd/SDD-ORCHESTRATOR.md`](../../sdd/SDD-ORCHESTRATOR.md) |
| Skill backend (paridade contratos) | [`docs/skills/backend-skill/SKILL.md`](../backend-skill/SKILL.md) |

---

## 3. Princípios (fusão Senior React × pragmatismo EmpregaNet)

| Princípio | Prática obrigatória |
| --------- | -------------------- |
| **SRP nos componentes** | Componente faz **uma** parcela óbvia de UI; dados/efeitos vão para hooks/services. |
| **Backend é fonte de verdade** | Não re-implementar regras densas já garantidas pela API só “por conveniência de UI”; duplicação apenas para ergonomia (**com** parity Zod só na fronteira). |
| **Inversão de dependência na fronteira API** | Páginas/hooks chamam services (`src/services/` ou convenção igual) — evitar `fetch` disperso nos componentes. |
| **KISS/YAGNI** | Não criar `core/domain/` profundo até haver comportamento repetido com valor claro — mas **isolá-lo** antes de segunda duplicação real. |
| **Type safety** | TypeScript **`strict`**; **proibido** `any`; `unknown` + *narrowing* quando preciso. |

---

## 4. Stack técnica (fechamento explícito)

| Usar | Não usar (neste projeto) |
| ------ | ---------------------------- |
| Next.js App Router + TypeScript | Tailwind novo ou aumento de uso |
| SCSS (módulos `.module.scss` quando existir convénio) | misturar múltiplos sistemas de estilo divergentes |
| Radix + ShadCN **adaptados a SCSS** | copiar verbatim kits que dependem só de Tailwind |
| React Hook Form + resolver Zod | validação apenas no submit sem mensagens tratadas |
| Zod para validar payloads e env vars sensíveis | confiar sempre em texto cru da rede |

> **Legado Tailwind:** se existir código antigo que já usa Tailwind, não expandir esse padrão; migrar apenas com tarefa explícita.

---

## 5. Arquitectura de pastas / responsabilidades

| Camada lógica | Onde típico | Regra |
| --------------- | ------------ | ----- |
| UI pura (“dumb”) | `components/` + subpastas primitives | só props/handlers declarados; não side-effects escondidos |
| Feature cohesion | `features/<nome>/...` quando criares área nova consistente | colocate hooks, wrappers de pagina específicos, assets locais |
| API + schemas | `src/services/<domínio>/`*(`*-api.ts`, `*-schema.ts` conforme projeto)* | Zod primeiro contacto ao JSON entrante ou env vars |
| Cross-feature UI helpers | apenas se **3+ consumidores** confirmados | senão duplication controlada até estabilizar |

---

## 6. Estado, dados e comunicação servidor

| Tópico | Expectativa |
| ------ | ----------- |
| **Loading / error / empty** | Sempre tratados visualmente explicitamente (+ retry onde UX exigir) |
| **Mutations idempotentes** | Evitar POST duplo: disable progressivo botão debounce/leveraging server idempotência |
| **Optimistic UI** | Só com caminho compensatório quando falhar request — não esconder erros silenciosos |
| **SSE / tempo real** | hook dedicado quando produto usar: reconexão, backoff UI explicável, cancel on unmount |

---

## 7. Autenticação e RBAC

- Middleware/App Router shields para rotas sensíveis (seguindo arquitectura existente).
- **Capacidades** centralizadas; evitar *strings mágicas* espalhadas — extrair enums/helpers compartilhados.
- UI condicional menus/ações sempre coerentes com papel real do backend (**nunca** apenas esconder link).

---

## 8. UX, estética e acessibilidade

| Âmbito | Orientação |
| ------ | ----------- |
| Leiaute e espaçamentos | Seguir grid/design system existente; evitar valores arbitrários que quebrem harmonização com o resto da UI. |
| Interação | Estados de foco e navegação por teclado; modais prendem e devolvem foco corretamente. |
| a11y | HTML semântico; ícones com `aria-label`; imagens com texto alternativo. |
| Reduced motion | Respeitar preferências já suportadas no projeto (hooks/utilitários existentes). |

Feedback visual sempre claro (**loading/disabled/errors** durante requests).

---

## 9. Documentação em TypeScript / hooks

Hooks ou utilidades **não triviais**:

```typescript
/**
 * Descreve intenção, parâmetros e retorno quando o nome sozinho não basta.
 * Texto pode ser PT-BR alinhando copy do produto.
 */
```

Evitar JSDoc barroco em wrappers de uma linha.

---

## 10. Testes

### Hierarquia de valor

| Prioridade | Ferramentas / foco |
| ---------- | ------------------ |
| 1º | Behavioral tests com Testing Library (**usuário/evento/DOM**) — mocking network via padrações do projeto (MSW/axios mocks se já existentes) |
| 2º | Snapshots apenas layout extremamente estável e pequenos |
| 3º | E2E `cypress` / `playwright` **somente quando diretório & pipeline existirem no repo |

### Diretrizes E2E herdadas das boas referências

| Regra | Detalhe |
| ----- | ------- |
| Estabilidade de selectores | preferir **`data-testid`** sem depender só de classe CSS efémeras |
| Isolamento | não depender ordenação implícito de suites sem `cy.session` equivalente já modelado |
| Interceptações | mocks de erro/sucesso externos para flaky reduzidos |

Adaptar comandos aos ficheiros reais sob `**/cypress/**` ou `**/e2e/**` só **se** existirem.

---

## 11. Checklist de entrega (feature UI)

1. [ ] Pasta feature coesa + componentes suficientemente pequenos.
2. [ ] Service/API layer Zod-validada na fronteira.
3. [ ] Estados **loading/error/empty retry** tratados onde há fetch.
4. [ ] Form com RHF + Zod + mensagens utilizador PT-BR.
5. [ ] a11y navegação básica (tab order overlays).
6. [ ] Testes comportamentais atualizados/criados (quando infra presente).

---

## 12. Anti-patterns

| Bloqueado | Motivo |
| --------- | ------- |
| Lógica de negócio densa JSX | impossibilita reuso/test isolado |
| Introdução ou expansão de Tailwind | política atual explícita |
| `fetch` espalhado sem service | regressão rápida de contratos inconsistentes |

---

## 13. Idioma

Copy UI usuário final preferencialmente **português Brasil** onde produto assim definir — nomes código **inglês**.

---

## Histórico versão skill

| Versão | Mudança |
| ------ | ------- |
| 2.0.0 | Formalização estrutura + boas-práticas agnósticas fundidas aos guardrails EmpregaNet |
