---
name: frontend-skill
description: Convenções canónicas do frontend Next.js do EmpregaNet — App Router com cacheComponents, TypeScript strict sem any, SCSS Modules + Radix/ShadCN adaptado (sem Tailwind), Zod + React Hook Form, service por feature, auth por cookie httpOnly e RBAC compartilhado entre proxy e guard, componentes canónicos de loading e testes BDD com Cucumber. Use ao ler, escrever ou revisar qualquer coisa em frontend/, incluindo páginas, componentes, hooks, cliente HTTP, estilos e acessibilidade. Não use para backend .NET (backend-skill) nem para regressão pela UI real (e2e-qa-skill).
---

# Frontend (Next.js — monorepo EmpregaNet)

Base de **conhecimento** do frontend: decisões fechadas, armadilhas já pagas em produção e checklists.
O comportamento de implementação está no agent `frontend-engineer`, que carrega esta skill como contexto obrigatório.

---

## 1. Quando aplicar

| Situação | Aplicar |
| -------- | ------- |
| Todo o trabalho em `frontend/` (App Router) | Sim |
| Novas páginas, componentes, hooks, estilos, proxy/rotas | Sim |
| Integração com a API + validação ao renderizar dados | Sim |
| Refactors que dividem componentes grandes ou corrigem a11y | Sim |
| Handlers, EF Core, contratos do lado do servidor | Não — [`backend-skill`](../backend-skill/SKILL.md) |
| Validar o comportamento real navegando a UI | Não — [`e2e-qa-skill`](../e2e-qa-skill/SKILL.md) |

---

## 2. Ligações

| Recurso | Path |
| ------- | ---- |
| Mapa do monorepo e comandos de build | [`docs/README.md`](../../../docs/README.md) |
| Agent de implementação frontend | [`frontend-engineer`](../../agents/frontend-engineer.md) |
| Paridade de contratos com a API | [`backend-skill`](../backend-skill/SKILL.md) |
| Governo SDD (quando há pasta `docs/features/<id>/`) | [`docs/sdd/SDD-ORCHESTRATOR.md`](../../../docs/sdd/SDD-ORCHESTRATOR.md) |
| Contratos request/response no frontend | [ADR 0009](../../../docs/sdd/adrs/0009-contratos-request-response-no-frontend.md) |

---

## 3. Princípios

| Princípio | Prática obrigatória |
| --------- | ------------------- |
| **SRP nos componentes** | Componente faz **uma** parcela óbvia de UI; dados/efeitos vão para hooks/services. |
| **Backend é fonte de verdade** | Não re-implementar regras densas já garantidas pela API; duplicação só para ergonomia, com paridade Zod na fronteira. |
| **Inversão de dependência na fronteira** | Páginas/hooks chamam o `service/` da feature dona — sem `fetch`/axios disperso em componentes. |
| **KISS/YAGNI** | Não criar `core/domain/` profundo antes de haver comportamento repetido com valor claro — mas isolá-lo **antes** da segunda duplicação real. |
| **Type safety** | TypeScript `strict`; **proibido `any`**; `unknown` + narrowing quando necessário. |

---

## 4. Stack (fechamento explícito)

| Usar | Não usar neste projecto |
| ---- | ----------------------- |
| Next.js 16 App Router + React 19 + TypeScript | Tailwind novo ou expansão do existente |
| SCSS Modules (`.module.scss`) e tokens em `globals.scss` | Misturar sistemas de estilo divergentes |
| Radix + ShadCN **adaptados a SCSS** | Copiar verbatim kits que dependem de Tailwind |
| React Hook Form + resolver Zod | Validação só no submit sem mensagens tratadas |
| Zod para payloads de entrada, de saída e env vars | Confiar em texto cru da rede |

> **Legado Tailwind:** se existir código antigo que já use Tailwind, não expandir; migrar apenas com tarefa explícita.

---

## 5. Pastas e responsabilidades

| Camada lógica | Onde | Regra |
| ------------- | ---- | ----- |
| UI pura ("dumb") | `src/shared/components/ui/` e `form-fields/` | Só props/handlers declarados; sem side-effects escondidos |
| Coesão por feature | `src/features/<domínio>/<sub>/` | Colocar hooks, wrappers de página e assets locais junto |
| API + schemas | **`src/features/<domínio>/service/`** — `*-api.ts`, `*-schema.ts`, `*-queries.ts`, `*-keys.ts` | Zod no primeiro contacto com o JSON entrante e nos env vars |
| Infra transversal | `src/shared/api/` (axios + interceptors), `src/shared/auth/` (sessão) | Não duplicar cliente HTTP por feature |
| Moldura da aplicação | `src/shared/shell/` (`AppShell`, sidebar, header, guard) | Shell **único** para rotas públicas e autenticadas |
| Helpers de UI cross-feature | Só com **3+ consumidores** confirmados | Senão, duplicação controlada até estabilizar |

> `src/services/` **não existe mais** — o service vive na feature dona. Infra compartilhada em `src/shared/api` + `src/shared/auth`.

Features existentes: `admin`, `auth`, `candidaturas`, `conta`, `dashboard`, `recrutamento`, `vagas`.

### 5.1 Renderização: Server vs Client Components

O projecto roda **Next 16 com `cacheComponents: true`** (`next.config.ts`). Regras aprendidas em produção — desrespeitar quebra a build:

| Regra | Detalhe |
| ----- | ------- |
| **Padrão = Client + React Query** | Áreas autenticadas (`(main)`) usam CSR com `useQuery`. Não migrar sem motivo. |
| **Server Component quando há SEO** | Páginas públicas indexáveis (catálogo de vagas) usam Server Component + `generateMetadata` dinâmica. |
| **O guard é que bloqueia o SSR** | `RouteAccessGuard` devolve spinner até hidratar, descartando o conteúdo. `AppShell` **não** bloqueia — renderiza `{children}` incondicionalmente, então um Client Component pode envolver conteúdo server-rendered. Rotas públicas ficam em `(public)`: mesmo `AppShell`, **sem** guard. |
| **`<Suspense>` acima do shell** | O boundary tem de ficar **acima** do Client Component de layout. Declarado abaixo dele (no layout ou na página) **não** satisfaz o `cacheComponents` → `Uncached data was accessed outside of <Suspense>`. |
| **Nada de "agora" no prerender** | `new Date()`, `Math.random()` e afins durante o prerender congelam o valor no build. Resolver no cliente (`useHasMounted`) ou dentro de boundary dinâmico. |
| **Módulo server-only isolado** | Ficheiros com `'use cache'`/`next/cache` levam `import 'server-only'` e **nunca** entram em barrel consumido por Client Components (ex.: `jobs-server.ts` fora de `service/index.ts`). |
| **Um fluxo por recurso** | Se o servidor já buscou o dado, passe-o por **prop** — não refaça `useQuery` do mesmo recurso no cliente. |

Diagnóstico de erro de prerender:

```bash
pnpm --dir frontend exec next build --debug-prerender
```

---

## 6. Estado, dados e comunicação com o servidor

| Tópico | Expectativa |
| ------ | ----------- |
| **Loading / error / empty** | Sempre tratados visualmente, com retry onde a UX exigir |
| **Mutations idempotentes** | Evitar POST duplo: desabilitar botão progressivamente, debounce, ou idempotência no servidor |
| **Optimistic UI** | Só com caminho compensatório em caso de falha — nunca esconder erros |
| **SSE / tempo real** | Hook dedicado quando o produto usar: reconexão, backoff explicável, cancel on unmount |

---

## 7. Autenticação e RBAC

- **Auth é 100% cookie `httpOnly`** — nenhum token em JS. O proxy (`src/proxy.ts`) lê o cookie `access_token`; em produção o `Domain` do cookie tem de ser compartilhado entre front e API. Ver [ADR 0001](../../../docs/sdd/adrs/0001-auth-por-cookies-httponly.md).
- **Uma única política de acesso:** o proxy edge (`src/proxy.ts`) e o guard cliente (`shared/shell/route-access-guard.tsx`) consomem o **mesmo** `evaluateRouteAccess` de `shared/utils/lib/route-access-policy.ts`. **Não criar uma terceira implementação.**
- Rota nova exige actualizar `isPublicPath` / `canAccessPath`; forbidden vai para `(status)/nao-autorizado`; a query `redirect` é canónica.
- **Capacidades** centralizadas; sem strings mágicas espalhadas — extrair enums/helpers compartilhados.
- UI condicional (menus/acções) coerente com o papel real vindo do backend — **nunca** apenas esconder o link.
- `AuthProvider` único, em `AppProviders`.

### 7.1 Auth ≠ dados do utilizador

| Responsabilidade | Onde | Endpoints |
| ---------------- | ---- | --------- |
| **Credencial e sessão** (entrar, sair, registar, renovar, recuperar acesso) | `features/auth/service/` e `shared/auth/` | `/api/auth/*` |
| **Dados do próprio utilizador** (ver/editar perfil, trocar senha, encerrar conta) | `features/conta/service/` | `/api/users/me*` |
| **Utilizadores como dado de negócio** (gestão, listagens) | `features/admin/usuarios/`, `features/recrutamento/candidatos/` | `/api/users`, `/api/admin/*` |

- O schema do utilizador (`userSchema`/`UserDto`) vive em **`shared/schema/user-schema.ts`**, não em `shared/auth`: descreve o utilizador como *dado*, consumido por telas sem relação com autenticação. Em `shared/auth` ficam só os contratos de credencial (`userLoggedSchema`, `refreshTokenSchema`).
- O interceptor de 401 (`shared/api/axios-auth.ts`) identifica as rotas de sessão **por caminho literal** (`/auth/refresh-token`, `/auth/logout`) para não tentar renovar a sessão em cima da própria renovação. Ao mover essas rotas, actualizar esse guard — senão a falha é silenciosa.

### 7.2 Contrato `userType`

Leitura = **Description em pt-BR**; escrita = **nome do enum**. Fonte única em `shared/utils/lib/user-types.ts` — não duplicar o mapeamento.

---

## 8. UX, estética e acessibilidade

| Âmbito | Orientação |
| ------ | ---------- |
| Layout e espaçamento | Seguir grid/tokens existentes; evitar valores arbitrários que quebrem a harmonia |
| Interacção | Estados de foco e navegação por teclado; modais prendem e devolvem foco |
| a11y | HTML semântico; ícones com `aria-label`; imagens com alternativa textual |
| Reduced motion | Respeitar as preferências já suportadas no projecto |

### 8.1 Estados de carregamento — componentes canónicos

Não criar spinner/placeholder novo: o design system já tem o vocabulário.

| Situação | Usar |
| -------- | ---- |
| **Troca de rota** dentro de um segmento | `loading.tsx` no segmento — **obrigatório** |
| Forma do conteúdo é **conhecida** (lista, formulário, detalhe) | `ListRowsSkeleton`, `FormFieldsSkeleton`, `DetailPageSkeleton` |
| Espera **indeterminada** (troca de rota, streaming de layout, sessão) | `LoadingState` (spinner + rótulo, centrado) |
| Indicador **embutido** (botão, campo, linha) | `Spinner` (`sm`/`md`/`lg`), herda `currentColor` |
| Submissão de formulário | `FormSubmitButton` — já desabilita, exibe `Spinner` e marca `aria-busy` |

Regras que devem ser mantidas nesses componentes:

- **Todo segmento com layout precisa de `loading.tsx`.** O boundary do layout envolve o **shell inteiro** — se a página suspender (chunk carregado sob demanda no primeiro acesso), sidebar e header são desmontados e remontados, causando piscada de tela cheia. O `loading.tsx` cria um boundary **dentro** do layout, em volta apenas da página, mantendo o shell montado. Os dois coexistem.
- **Atraso antes de aparecer:** `LoadingState` só fica visível após ~250 ms. Indicador que pisca por 150 ms faz a UI parecer instável.
- **Sem trocar o rótulo do botão** por "Enviando…": muda a largura e desloca o layout. Adiciona-se o spinner e mantém-se o texto.
- **`prefers-reduced-motion`:** não congelar o anel (parece quebrado) — trocar rotação por pulsação de opacidade.
- Contentor com `role="status"`; texto `sr-only` quando não há rótulo visível.

---

## 9. Documentação em TypeScript

Hooks e utilidades **não triviais** levam JSDoc curto explicando intenção, parâmetros e retorno.
Evitar JSDoc barroco em wrappers de uma linha.

---

## 10. Testes

### Infra real hoje

O frontend usa **Cucumber (BDD)** em `frontend/tests/` — `specs/` (`.feature`) + `steps/` + `support/`.

```bash
pnpm --dir frontend test
```

Perfis: `pnpm test:unit` e `pnpm test:integration`.

**Testing Library, Jest, Cypress e Playwright NÃO estão instalados.** Não escrever testes que os assumam nem sugeri-los como se existissem; propor a adição é decisão explícita.

### Hierarquia de valor

| Prioridade | Foco |
| ---------- | ---- |
| 1º | Lógica pura de alto risco em `.feature` + steps: schemas Zod e mappers, regras de negócio, controlo de acesso a rotas, máscaras/normalizações |
| 2º | Fluxos de formulário (validação → payload da API) como cenários de integração |
| 3º | Comportamento que só aparece com a app a correr (navegação real, layout, timing, integração ao vivo) → **não é Cucumber**: é a regressão manual pela UI, coberta por [`e2e-qa-skill`](../e2e-qa-skill/SKILL.md) |

Cenários existentes cobrem `route-access-control`, regras de vaga/empresa, UF/actividade e máscara de telefone — seguir esse padrão ao adicionar lógica pura nova. **Não** duplicar em Cucumber o que a regressão de browser já cobre, nem o contrário.

---

## 11. Validação (comandos reais)

```bash
pnpm --dir frontend lint
```

```bash
pnpm --dir frontend test
```

```bash
pnpm --dir frontend build
```

Se o diff tocou renderização, conferir a classificação de rota no output do build (`○` estático / `◐` parcial).

---

## 12. Checklist de entrega (feature UI)

1. [ ] Pasta de feature coesa; componentes suficientemente pequenos.
2. [ ] `service/` da feature com Zod na fronteira — entrada **e** payload de saída.
3. [ ] Estados **loading/error/empty/retry** com os componentes canónicos (§8.1) — sem spinner ad-hoc.
4. [ ] Form com RHF + Zod e mensagens ao utilizador em **pt-BR**.
5. [ ] a11y: navegação por teclado, foco em overlays, `prefers-reduced-motion`.
6. [ ] Rota nova → `isPublicPath`/`canAccessPath` actualizados; proxy e guard continuam a usar `evaluateRouteAccess`.
7. [ ] `lint` + `test` + `build` verdes (§11); classificação de rota conferida se tocou renderização.
8. [ ] Cenários Cucumber criados/actualizados quando há lógica pura nova.

---

## 13. Anti-padrões

| Bloqueado | Motivo |
| --------- | ------ |
| Lógica de negócio densa no JSX | Impede reuso e teste isolado |
| Introduzir ou expandir Tailwind | Política actual explícita |
| `fetch`/axios espalhado sem service | Regressão rápida de contratos inconsistentes |
| Reexportar módulo `server-only` em barrel de feature | Arrasta `next/cache` para o bundle do cliente e quebra a build |
| Buscar o mesmo recurso no servidor **e** com `useQuery` | Duplica responsabilidade; passe por prop |
| Spinner/skeleton ad-hoc | Já existem `Spinner`, `LoadingState` e os skeletons (§8.1) |
| Terceira implementação de política de rota | Proxy e guard partilham `evaluateRouteAccess` |
| Token de auth em JS / `js-cookie` para sessão | A API já emite `httpOnly`; duplicar cria cookie fantasma |
| `any` em TypeScript | `strict` é regra do projecto |
| Texto de UI em português europeu | Produto é **pt-BR**, incluindo `sr-only` e mensagens de erro |

---

## 14. Idioma

Copy de utilizador final em **português (Brasil)**; identificadores de código em **inglês**.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 3.0.0 | Movida para `.claude/skills/` (passa a ser carregável); resolvida a contradição sobre E2E de browser (§10) que a marcava como inexistente; acrescentados cookie httpOnly, contrato `userType`, política única de rota, secção de validação com comandos reais e anti-padrões correspondentes |
| 2.1.0 | `service/` por feature, regras de Server vs Client com `cacheComponents`, componentes canónicos de loading, infra Cucumber |
| 2.0.0 | Formalização de estrutura + boas práticas fundidas aos guardrails EmpregaNet |
