---
name: frontend-skill
description: >-
  Skill completa EmpregaNet Frontend: Next.js App Router, TypeScript estrito, SCSS + Radix/ShadCN sem Tailwind,
  Zod e React Hook Form, service por feature, auth/RBAC, Server vs Client Components (cacheComponents),
  estados de carregamento canónicos e testes BDD com Cucumber.
  Use ao criar ou alterar UI, hooks, cliente HTTP, fluxos ou acessibilidade no monorepo frontend.
author: EmpregaNet
version: 2.1.0
date: 2026-07-25
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
| **Inversão de dependência na fronteira API** | Páginas/hooks chamam o `service/` da feature dona — evitar `fetch`/axios disperso nos componentes. |
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
| UI pura (“dumb”) | `src/shared/components/ui/` (atoms/molecules/organisms) e `form-fields/` | só props/handlers declarados; não side-effects escondidos |
| Feature cohesion | `src/features/<domínio>/<sub>/` | colocate hooks, wrappers de página específicos, assets locais |
| API + schemas | **`src/features/<domínio>/service/`** — `*-api.ts`, `*-schema.ts`, `*-queries.ts`, `*-keys.ts` | Zod no primeiro contacto com o JSON entrante e nos env vars |
| Infra transversal | `src/shared/api/` (axios + interceptors), `src/shared/auth/` (sessão) | não duplicar cliente HTTP por feature |
| Moldura da aplicação | `src/shared/shell/` (`AppShell`, sidebar, header, guard) | shell **único** para rotas públicas e autenticadas |
| Cross-feature UI helpers | apenas se **3+ consumidores** confirmados | senão duplicação controlada até estabilizar |

> `src/services/` **não existe mais** — o service vive na feature dona. Infra compartilhada fica em `src/shared/api` + `src/shared/auth`.

---

## 5.1 Renderização: Server vs Client Components

O projeto roda **Next 16 com `cacheComponents: true`** (`next.config.ts`). Regras aprendidas em produção — desrespeitar quebra a build:

| Regra | Detalhe |
| ----- | ------- |
| **Padrão = Client + React Query** | Áreas autenticadas (`(main)`) usam CSR com `useQuery`. Não migrar sem motivo. |
| **Server Component quando há SEO** | Páginas públicas indexáveis (catálogo de vagas) usam Server Component + `generateMetadata` dinâmica. |
| **O guard é que bloqueia o SSR** | `RouteAccessGuard` devolve spinner até hidratar, descartando o conteúdo. `AppShell` **não** bloqueia — renderiza `{children}` incondicionalmente, então Client Component pode envolver conteúdo server-rendered. Rotas públicas ficam em `(public)`: mesmo `AppShell`, **sem** guard. |
| **`<Suspense>` acima do shell** | O boundary tem de ficar **acima** do Client Component de layout. Declarado abaixo dele (no layout ou na página) **não** satisfaz o `cacheComponents` → `Uncached data was accessed outside of <Suspense>`. |
| **Nada de "agora" no prerender** | `new Date()`, `Math.random()` e afins durante o prerender congelam o valor no build. Resolver no cliente (`useHasMounted`) ou dentro de boundary dinâmico. |
| **Módulo server-only isolado** | Ficheiros com `'use cache'`/`next/cache` levam `import 'server-only'` e **nunca** entram em barrel consumido por Client Components (ex.: `jobs-server.ts` fora de `service/index.ts`). |
| **Um fluxo por recurso** | Se o servidor já buscou o dado, passe-o por **prop** — não refaça `useQuery` do mesmo recurso no cliente. |

Diagnóstico de erro de prerender: `pnpm exec next build --debug-prerender` mostra o stack real.

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

### Estados de carregamento — componentes canónicos

Não criar spinner/placeholder novo: o design system já tem o vocabulário.

| Situação | Usar |
| -------- | ---- |
| **Troca de rota** dentro de um segmento | `loading.tsx` no segmento — **obrigatório** |
| Forma do conteúdo é **conhecida** (lista, formulário, detalhe) | `ListRowsSkeleton`, `FormFieldsSkeleton`, `DetailPageSkeleton` — comunicam o que vem e evitam salto de layout |
| Espera **indeterminada** (troca de rota, streaming de layout, sessão) | `LoadingState` (spinner + rótulo, centrado) |
| Indicador **embutido** (botão, campo, linha) | `Spinner` (`sm`/`md`/`lg`); herda `currentColor`, adapta-se ao fundo |
| Submissão de formulário | `FormSubmitButton` já desabilita, exibe `Spinner` e marca `aria-busy` — não reimplementar |

Regras de UX/a11y aplicadas nesses componentes e que devem ser mantidas:

- **Todo segmento com layout precisa de `loading.tsx`.** O boundary do layout envolve o **shell inteiro** — se a página suspender (chunk carregado sob demanda no primeiro acesso à rota), sidebar e header são desmontados e remontados, causando piscada de tela cheia. O `loading.tsx` cria um boundary **dentro** do layout, em volta apenas da página, mantendo o shell montado. Os dois coexistem: o do layout cobre a carga inicial, o do segmento cobre a navegação.
- **Atraso antes de aparecer:** `LoadingState` só fica visível após ~250ms. Indicador que pisca por 150ms faz a UI parecer instável.
- **Sem trocar o rótulo do botão** por "Enviando…": muda a largura e desloca o layout. Adiciona-se o spinner e mantém-se o texto.
- **`prefers-reduced-motion`:** não congelar o anel (parece quebrado) — trocar rotação por pulsação de opacidade.
- Contentor com `role="status"`; texto `sr-only` quando não há rótulo visível.

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

### Infra real hoje

O frontend usa **Cucumber (BDD)** em `frontend/tests/` — `specs/` (`.feature`) + `steps/` + `support/`. Perfis:

```bash
pnpm test              # tudo
pnpm test:unit         # perfil unit
pnpm test:integration  # perfil integration
```

**Testing Library, Jest, Cypress e Playwright NÃO estão instalados.** Não escrever testes que os assumam nem sugeri-los como se existissem; propor a adição é uma decisão explícita, não um pressuposto.

### Hierarquia de valor

| Prioridade | Foco |
| ---------- | ---- |
| 1º | Lógica pura de alto risco em `.feature` + steps: schemas Zod e mappers, regras de negócio, controlo de acesso a rotas, máscaras/normalizações |
| 2º | Fluxos de formulário (validação → payload da API) como cenários de integração |
| 3º | E2E em navegador — **ainda inexistente**; se for adicionado, criar diretório e pipeline antes de escrever cenários |

Cenários existentes cobrem `route-access-control`, regras de vaga/empresa, UF/atividade e máscara de telefone — seguir esse padrão ao adicionar lógica pura nova.

---

## 11. Checklist de entrega (feature UI)

1. [ ] Pasta feature coesa + componentes suficientemente pequenos.
2. [ ] `service/` da feature Zod-validado na fronteira (entrada **e** payload de saída).
3. [ ] Estados **loading/error/empty/retry** com os componentes canónicos (§8) — sem spinner ad-hoc.
4. [ ] Form com RHF + Zod + mensagens ao utilizador em **pt-BR**.
5. [ ] a11y: navegação por teclado, foco em overlays, `prefers-reduced-motion`.
6. [ ] Se tocou renderização: build limpo (`pnpm build`) e classificação de rota conferida (`○`/`◐`).
7. [ ] Cenários Cucumber criados/atualizados quando há lógica pura nova.

---

## 12. Anti-patterns

| Bloqueado | Motivo |
| --------- | ------- |
| Lógica de negócio densa JSX | impossibilita reuso/test isolado |
| Introdução ou expansão de Tailwind | política atual explícita |
| `fetch` espalhado sem service | regressão rápida de contratos inconsistentes |
| Reexportar módulo `server-only` em barrel de feature | arrasta `next/cache` para o bundle do cliente e quebra a build |
| Buscar o mesmo recurso no servidor **e** com `useQuery` | duplica responsabilidade; passe por prop |
| Spinner/skeleton ad-hoc | já existem `Spinner`, `LoadingState` e os skeletons (§8) |
| Texto de UI em português europeu | produto é **pt-BR** — inclui `sr-only` e mensagens de erro |

---

## 13. Idioma

Copy UI usuário final preferencialmente **português Brasil** onde produto assim definir — nomes código **inglês**.

---

## Histórico versão skill

| Versão | Mudança |
| ------ | ------- |
| 2.1.0 | Alinha à estrutura real: `service/` por feature (fim de `src/services/`), regras de Server vs Client Components com `cacheComponents`, componentes canónicos de loading, infra de testes Cucumber (sem Testing Library) |
| 2.0.0 | Formalização estrutura + boas-práticas agnósticas fundidas aos guardrails EmpregaNet |
