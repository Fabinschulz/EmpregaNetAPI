---
name: frontend-skill
description: Convenções canónicas do frontend Next.js do EmpregaNet — App Router com cacheComponents, TypeScript strict sem any, SCSS Modules + Radix/ShadCN adaptado (sem Tailwind), Zod + React Hook Form, service por feature, auth por cookie httpOnly e RBAC compartilhado entre proxy e guard, YAGNI aplicado (quatro custos, quando promover um componente a primitivo partilhado), componentes canónicos de loading e testes BDD com Cucumber. Use ao ler, escrever ou revisar qualquer coisa em frontend/, incluindo páginas, componentes, hooks, cliente HTTP, estilos e acessibilidade, e ao decidir se uma abstracção, variante ou primitivo se cria agora ou se adia. Não use para backend .NET (backend-skill) nem para regressão pela UI real (e2e-qa-skill).
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
| **KISS/YAGNI** | Não criar `core/domain/` profundo antes de haver comportamento repetido com valor claro — mas isolá-lo **antes** da segunda duplicação real (§3.1). |
| **Type safety** | TypeScript `strict`; **proibido `any`**; `unknown` + narrowing quando necessário. |

### 3.1 YAGNI — o que não se constrói agora

Critério de **Martin Fowler** ([`martinfowler.com/bliki/Yagni.html`](https://martinfowler.com/bliki/Yagni.html)),
aplicado a este frontend. **Feature presumida** é capacidade construída hoje para uma necessidade suposta
amanhã — e YAGNI aplica-se só a isso.

Quatro custos, não só o primeiro:

| Custo | O que é | Sintoma aqui |
| ----- | ------- | ------------ |
| **Construir** | Esforço gasto na capacidade presumida | Componente, hook ou variante que nenhuma tela usa |
| **Atraso** | O que ficou por entregar enquanto se construía a presunção | Tela que escorregou porque se generalizou primeiro |
| **Carregar** | A complexidade extra torna **todo o resto** mais caro de mudar | Prop de configuração que cada consumidor tem de entender; abstracção atravessada em toda a alteração |
| **Reparar** | Quando a necessidade chega diferente do presumido, desfazer custa mais do que nunca ter feito | Primitivo partilhado que se torce para caber no segundo caso |

Ambos os desfechos perdem: se não for precisa, paga-se construir + carregar; se for precisa mas diferente,
paga-se construir + carregar + reparar, e a versão errada ainda enviesa a solução certa. O custo de **carregar**
é o que ninguém atribui à decisão que o originou.

**Limite do princípio — YAGNI corta capacidade, nunca qualidade.** Fowler é explícito: cobre capacidade para
feature presumida, **não** o esforço de manter o software fácil de modificar. Não serve para cortar estados de
carregamento/erro/vazio, acessibilidade, `strict` sem `any`, paridade Zod na fronteira ou o guard de rota —
isso é o comportamento real da tela, não capacidade futura.

**Custos assimétricos — aqui adiar é mais caro que construir:**

| Decisão | Porque adiar é caro |
| ------- | ------------------- |
| **Schema Zod na fronteira** de um `service/` | Schema errado propaga-se por toda a feature; validar depois obriga a refazer os tipos derivados |
| **Classificação Server vs Client** de uma página | Trocar depois arrasta a árvore de componentes e o comportamento de `cacheComponents` (§5.1) |
| **Política de rota e RBAC** | Página sensível exposta não se compensa depois; a política é única e partilhada entre proxy e guard (§7) |
| **Semântica e a11y da estrutura** | Retrofitar acessibilidade custa mais do que nascer correcta (§8) |

Fora desta lista, presunção vai para o backlog, não para o código. Casos concretos que **não** se constroem
por antecipação: `core/domain/` profundo antes de comportamento repetido; promover um componente a primitivo
de `shared/` enquanto houver **um** consumidor — enquanto houver um, vive na feature dona; estado global ou
Context antes de existir estado partilhado por mais de uma árvore; prop de configuração ou variante que
nenhuma tela passa hoje; generalizar na primeira variação, quando são duas ocorrências concretas que ensinam
a forma certa.

**Teste de decisão** — para cada peça sem consumidor hoje, uma resposta fraca basta para adiar:

1. Quem consome isto hoje? "Ninguém, mas..." é feature presumida.
2. Quanto custa acrescentar quando a necessidade chegar? Se cai na tabela acima, decidir agora com fundamento.
3. O que esta peça torna mais caro enquanto existir? É o custo de carregar — nomeá-lo.
4. Qual o gatilho concreto que a traz de volta? Sem gatilho nomeável, a necessidade é imaginada.

Registar a recusa numa linha, no PR ou no `tasks.md`:

> **Adiado:** `<capacidade>` — sem consumidor hoje; custo de adicionar depois é local a `<ficheiro/módulo>`;
> gatilho de retorno: `<evento concreto>`.

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

### 5.2 Atomic design em `shared/components/ui`

A biblioteca de UI é organizada em quatro tiers, e a dependência aponta **sempre para baixo**:

`atoms/` ← `molecules/` ← `organisms/` ← `templates/` (as *pages* são as rotas em `src/app`).

| Tier | Critério objectivo | Exemplos reais |
| ---- | ------------------ | -------------- |
| `atoms/` | Envolve **um** elemento/primitivo. Não importa nenhum outro componente. Um valor, um alvo de foco | `Button`, `Input`, `Textarea`, `Badge`, `Label`, `Skeleton`, `Spinner`, `CardSectionLabel` |
| `molecules/` | Pequeno grupo que funciona como unidade. Compõe átomos ou primitivos Radix | `Card`, `PageHeader`, `StatusBadge`, `Select`, `Popover`, `Tooltip`, `entity-card/*`, `FloatingThemeToggle` |
| `organisms/` | Seção complexa, com estado ou layout próprio | `DataTable`, `TableContainer`, `FormLayout`, `FilterBar`, `ChoiceGroup` |
| `templates/` | Esqueleto de página: moldura e slots, sem conteúdo de domínio | `CenteredPageFrame` |

Decisões fechadas, para não reabrir a discussão a cada componente novo:

- **`Input` é átomo, não molécula.** A API é `ComponentProps<'input'>`, ele não importa outro
  componente, e os adornos são *slots*. O toggle de senha é comportamento do próprio controle,
  como o date picker nativo — não promove o tier.
- **A molécula de formulário é `FormField`/`InputField`** (em `form-fields/`, fora de `ui/`):
  rótulo + controle + erro + dica. Se `Input` fosse molécula, os dois níveis colapsariam.
- **Rótulo é tier, não tamanho.** `ChoiceGroup` é organismo por ter estado, busca e subcomponentes
  internos; `Card` é molécula apesar de 6 exports, porque não compõe nada.

A escada é **executada por lint**, não é convenção de pasta: `no-restricted-imports` em
`eslint.config.mjs` (`UI_TIER_ORDER` / `uiTierRule`) rejeita import ascendente em qualquer forma —
alias (`@/shared/components/ui/organisms/...`) ou relativa (`../../organisms/...`). Para inverter
uma dependência, o tier de cima compõe o de baixo passando conteúdo por prop/slot.

Dentro de `ui/`, **nunca** importar `@/shared/components` nem `@/shared/components/ui`: um membro do
barril a passar pelo próprio barril fecha um ciclo. Use o caminho relativo do tier
(`../../atoms/button`). O lint também bloqueia isto.

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

### 8.1 Layout de formulário — componentes canónicos

Todo formulário de cadastro/edição dentro do `AppShell` usa os primitivos de
`shared/components/ui/organisms/form-layout`. Não desenhar grid nem barra de ações por feature.

| Componente | Papel |
| ---------- | ----- |
| `FormHeader` | Título + descrição à esquerda, **ações à direita** (Voltar, secundárias, primária). Fica *sticky* no topo (estático no telefone). Sem `title`, sai só a barra de ações — para o formulário que vive dentro de um `Card` cujo `CardHeader` já tem título |
| `FormActions` | Rodapé de ações — **só** para o formulário curto de auto-serviço (ver exceção abaixo) |
| `FormSection` | `fieldset` + `legend` (+ `description` opcional) com grid próprio: `cols={1..4}` |
| `FormRow` | Linha de campos avulsa, `cols={1..4}` (padrão 2) |
| `FormCol` | Largura relativa de um campo: `span={2}`, ou `span="full"` para linha inteira em qualquer tela |
| `FormNotice` | Faixa dos alertas da API, entre cabeçalho e campos |

Regras que devem ser mantidas:

- **Ações no cabeçalho, nunca num rodapé.** Num formulário longo, Salvar no fim obriga a percorrer
  tudo para descobrir se há como gravar. Como o `FormHeader` é renderizado dentro do `FormProvider`,
  o botão primário é o `submit` do próprio `<form>` — sem `form="id"` nem handler avulso.
- **Aproveitar a largura em desktop:** 4 colunas quando os campos comportam, 3 para campos médios,
  `span` maior só para o que precisa (logradouro, descrição). Campo curto (CEP, número) não ocupa a
  largura de um logradouro.
- **Colapso responsivo é do primitivo:** desktop usa `cols`, tablet limita a 2 e o telefone empilha.
  `span` numérico é proporcional e volta a uma célula quando o grid encolhe; só `span="full"` fica
  inteiro em todas as telas. Não escrever media queries por feature.
- **Agrupar por seção** com título (e descrição quando o título não basta), em vez de uma sequência
  única de campos. Criação e edição da mesma entidade partilham o componente de campos.
- **Filtros de listagem seguem o irmão deste padrão:** `FilterBar` + `FilterField span`.

Exceção registada — **formulário curto dentro de um `Card`**: conta (perfil, segurança) e tipo de
usuário (admin). São formulários de 1 a 3 campos, visíveis por inteiro sem rolar. Ficam com
`FormGrid narrow` (coluna de 640px, espaçamento compacto) e `FormActions` no rodapé, onde a ação é o
passo natural no fim da leitura. Espalhar três campos por 1000px ali só afasta rótulo de valor, e
ação no cabeçalho não resolve rolagem que não existe. **Não migrar essas telas para `FormHeader`.**
O critério é o tamanho, não a rota: formulário que precisa de seções e rolagem usa `FormHeader`.

### 8.2 Estados de carregamento — componentes canónicos

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
3. [ ] Estados **loading/error/empty/retry** com os componentes canónicos (§8.2) — sem spinner ad-hoc.
4. [ ] Form com RHF + Zod e mensagens ao utilizador em **pt-BR**.
5. [ ] a11y: navegação por teclado, foco em overlays, `prefers-reduced-motion`.
6. [ ] Rota nova → `isPublicPath`/`canAccessPath` actualizados; proxy e guard continuam a usar `evaluateRouteAccess`.
7. [ ] `lint` + `test` + `build` verdes (§11); classificação de rota conferida se tocou renderização.
8. [ ] Todo componente partilhado, prop ou variante introduzida tem **consumidor no mesmo diff**; o que foi adiado está registado com gatilho de retorno (§3.1).
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
| Spinner/skeleton ad-hoc | Já existem `Spinner`, `LoadingState` e os skeletons (§8.2) |
| Salvar/Voltar num rodapé de formulário de cadastro/edição | Obriga a percorrer o formulário todo para achar a ação; o lugar é o `FormHeader`. Conta é exceção registada (§8.1) |
| Grid de campos ou media query por feature | `FormSection`/`FormRow`/`FormCol` já resolvem colunas e colapso (§8.1) |
| Terceira implementação de política de rota | Proxy e guard partilham `evaluateRouteAccess` |
| Token de auth em JS / `js-cookie` para sessão | A API já emite `httpOnly`; duplicar cria cookie fantasma |
| `any` em TypeScript | `strict` é regra do projecto |
| Promover a `shared/` um componente com **um** consumidor; prop ou variante que nenhuma tela passa | Custo de carregar sem consumidor (§3.1) |
| Generalizar na primeira variação | Uma ocorrência mais uma hipótese ensina a forma errada (§3.1) |
| Invocar YAGNI contra a11y, estados de erro, `strict` ou paridade Zod | Fora do âmbito do princípio: corta capacidade, não qualidade (§3.1) |
| Texto de UI em português europeu | Produto é **pt-BR**, incluindo `sr-only` e mensagens de erro |

---

## 14. Idioma

Copy de utilizador final em **português (Brasil)**; identificadores de código em **inglês**.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 3.3.0 | YAGNI deixa de ser uma linha de tabela e passa a critério aplicável (§3.1): quatro custos de Fowler, limite do princípio, custos assimétricos deste frontend (schema Zod, classificação Server/Client, política de rota, a11y) e teste de decisão, com item de checklist e anti-padrões correspondentes |
| 3.2.0 | Atomic design em `shared/components/ui` (§5.2): quatro tiers com critério objectivo, `Input` fixado como átomo, e a escada de dependência executada por `no-restricted-imports` em vez de convenção de pasta |
| 3.1.0 | Layout canónico de formulário (§8.1): ações no `FormHeader`, grid responsivo por `FormSection`/`FormRow`/`FormCol`, com os anti-padrões correspondentes |
| 3.0.0 | Movida para `.claude/skills/` (passa a ser carregável); resolvida a contradição sobre E2E de browser (§10) que a marcava como inexistente; acrescentados cookie httpOnly, contrato `userType`, política única de rota, secção de validação com comandos reais e anti-padrões correspondentes |
| 2.1.0 | `service/` por feature, regras de Server vs Client com `cacheComponents`, componentes canónicos de loading, infra Cucumber |
| 2.0.0 | Formalização de estrutura + boas práticas fundidas aos guardrails EmpregaNet |
