---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Tasks — Filtros e persistência nas telas de candidaturas do recrutamento

**feature-id:** `emp-filtros-candidaturas-recrutamento` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.0.0 · **Spec:** [`spec.md`](spec.md) v1.0.0

**Ordem entre features:** esta é a primeira a ser implementada, porque introduz o hook
`useUrlSyncedParams` que a `emp-filtro-tipo-usuario-admin` reaproveita. A
`emp-filtros-vagas-localizacao` é independente e pode correr em paralelo.

Restrição de release: **API antes ou junto do frontend.** O frontend passa a enviar `status` e `search`
a endpoints que hoje os ignoram; com a API antiga, o filtro aparece na tela e não filtra nada.

## Bloco 1 — Backend: status e busca em `GET /api/jobapplications`

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 1.1 | Extrair `ApplicationStatusParser.ParseOrNull(string?)` a partir de `GetJobApplicationsByJobIdHandler.ParseStatus` (mesmo `DomainErrorEnum.INVALID_QUERY_FILTER`, rejeita `NaoSelecionado`); `GetJobApplicationsByJobIdHandler` passa a usá-lo sem mudança de comportamento | Application | CA-02 |
| 1.2 | `GetAllJobApplicationsQuery` dedicado (`Page, Size, OrderBy, IsDeleted, Search, Status`); `GetAllJobApplicationsHandler` passa a tratá-lo em vez de `GetAllQuery<JobApplicationViewModel>` | Application | CA-02, CA-03 |
| 1.3 | `JobApplicationRepository`: `JoinCandidateAndJob` + `ApplySearch`; `ProjectWithCandidate` passa a partir do resultado juntado; `GetAllWithCandidateAsync` ganha `status` e `search`; `GetByUserIdAsync` usa a mesma cadeia com `search: null` | Infra | CA-02, CA-03 |
| 1.4 | `IJobApplicationRepository.GetAllWithCandidateAsync`: parâmetros `status` e `search` opcionais | Domain | CA-02, CA-03 |
| 1.5 | `JobApplicationsController.GetAll`: deixa de chamar `base.GetAll`, monta o query dedicado e aceita `status` | Api | CA-02 |
| 1.6 | Confirmar que nenhum outro ponto do código despacha `GetAllQuery<JobApplicationViewModel>` (senão o mediator fica sem handler para ele) | Application | — |

## Bloco 2 — Backend: busca em `GET /api/jobapplications/job/{jobId}`

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 2.1 | `GetJobApplicationsByJobIdQuery` ganha `Search`; `GetByJobIdAsync` ganha `search` e aplica a cadeia do 1.3 | Application + Infra | CA-04 |
| 2.2 | `JobApplicationsController.GetByJob`: `[FromQuery] string? search` | Api | CA-04 |
| 2.3 | Atualizar `GetJobApplicationsByJobIdValidatorTests` se o validator existente precisar conhecer `Search` (limite de tamanho coerente com os outros endpoints) | Testes | — |

## Bloco 3 — Testes de backend

| # | Tarefa | CA |
| - | ------ | -- |
| 3.1 | `Unit/JobApplications/GetAllJobApplicationsHandlerTests`: adaptar ao query dedicado; status repassado; inválido e `NaoSelecionado` → `ValidationAppException` | CA-02 |
| 3.2 | `Integration/Handlers/GetAllJobApplicationsHandlerIntegrationTests` (novo): status; busca por nome, e-mail e título; combinação status + busca; isolamento por empresa com busca pelo nome exato; candidato excluído logicamente continua listado; `totalItems` reflete o filtro | CA-02, CA-03 |
| 3.3 | `Integration/Handlers/GetJobApplicationsByJobIdHandlerIntegrationTests`: busca isolada e busca + status | CA-04 |
| 3.4 | Rodar `GetMyJobApplicationsHandlerIntegrationTests` existente — deve seguir verde sem alteração | regressão |

## Bloco 4 — Frontend: infraestrutura compartilhada

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 4.1 | `shared/hooks/use-url-synced-params.ts` (`useUrlSyncedParams`) + export no barrel de `shared/hooks`: lê a URL no mount com fallback nos defaults, escreve com `router.replace(..., { scroll: false })`, omite valores iguais ao default, ignora valor fora do enum do schema | Frontend | CA-06, CA-07 |
| 4.2 | `frontend/tests/specs/unit/url-synced-params.feature`: ida e volta, defaults omitidos, valor inválido cai no default | Testes | CA-06 |
| 4.3 | `shared/schema/list-params.ts`: `JobApplicationsListQueryParams` ganha `SearchFilterParams`; `JobApplicationsAdminListQueryParams` ganha `StatusFilterParams` e `SearchFilterParams` | Frontend | CA-02, CA-03, CA-04 |

## Bloco 5 — Frontend: as três telas

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 5.1 | `/recrutamento/candidaturas`: schema com `status` + `search`; `RecruitmentApplicationsFilterFields` com Status, Buscar e "Limpar", migrado para `useFilterFormSync` (remove o `useEffect`/`isFirstRun` manual); `FilterSection` deixa de se chamar "Ordenar candidaturas"; página passa `status`/`search` à query e usa `useUrlSyncedParams` | Frontend | CA-01, CA-02, CA-03, CA-06, CA-08 |
| 5.2 | `/recrutamento/vagas/[id]/candidatos`: `search` no schema e em `candidatesFilterToParams`; campo Buscar; **botão "Limpar"** (hoje ausente nesta tela); `useUrlSyncedParams` | Frontend | CA-04, CA-06, CA-08 |
| 5.3 | `/recrutamento/vagas`: `orderBy` no schema (default `createdAt_DESC`), `SelectField` com `DATE_ORDER_BY_OPTIONS`, repasse à query; `useUrlSyncedParams` | Frontend | CA-05, CA-06 |
| 5.4 | Estado vazio com filtro ativo nas três telas: `emptyMessage` (já aceita `ReactNode`) passa a trazer texto explicativo + ação "Limpar filtros" quando algum filtro difere do default | Frontend | CA-09 |
| 5.5 | Trocar de filtro volta à página 1 nas três telas (mesmo `setPage(1)` que a tela de candidaturas já faz com `orderBy`) | Frontend | CA-02 |
| 5.6 | `frontend/tests/specs/unit/recruitment-applications-filters.feature` (novo): opções de Status, `'all'` fora dos params, `search` nos params das duas telas de candidaturas, `orderBy` nos params de vagas | Testes | CA-01, CA-04, CA-05 |

## Bloco 6 — Verificação

| # | Tarefa | Quem | CA |
| - | ------ | ---- | -- |
| 6.1 | Revisão do diff (foco: escopo de empresa preservado, handler órfão, composição da query antes da paginação) | `code-reviewer` | — |
| 6.2 | E2E nas três telas contra a API de desenvolvimento: filtros, busca, recarregar, detalhe e voltar, Limpar, estado vazio | `e2e-qa-skill` | CA-01…CA-09 |
| 6.3 | Preencher o estado de cada linha da matriz do `spec.md` com evidência | Orquestrador | todos |

Gates antes do PR: `dotnet build` + `dotnet test` (com `-p:BaseOutputPath` se a API estiver rodando),
`pnpm lint`, `pnpm test`, `pnpm format:check` nos arquivos tocados.

## Adiado

| Item | Gatilho de retorno |
| ---- | ------------------ |
| Filtro por vaga específica em `/recrutamento/candidaturas` | Busca textual por título de vaga não bastar para isolar uma vaga (relato de uso ou volume por recrutador) |
| Autocomplete que busca no backend (hoje sugere só itens da página carregada) nas telas administrativas | Tratado como correção à parte, fora do SDD |
| Índice trigram para a busca por `Contains` | Latência do endpoint aparecer como problema em métrica |
| Filtro por `isDeleted` na lista geral de candidaturas | Surgir caso de uso de manutenção com utilizador identificado |
| Gravar a URL com `window.history.replaceState` em vez de `router.replace` (fecha a janela em que uma escrita ainda pendente e um clique no menu para a mesma query que o estado já tem deixam estado e URL divergentes) | E2E ou relato de uso mostrarem a divergência na prática — hoje a janela é a duração de uma navegação e não foi medida |
| Preservar na escrita os parâmetros de query que não são do filtro (ex.: `utm_*`) | Alguma tela passar a receber parâmetros além dos do filtro |
| `PaginationExtensions.ToPaginatedListAsync` aplica `Distinct()` depois de `Take()`, gerando `SELECT DISTINCT` externo sem `ORDER BY` — o Postgres pode reordenar as linhas dentro da página (defeito anterior a esta feature, afeta as listas paginadas) | E2E mostrar ordem errada dentro da página em "Ordenar por", ou `EXPLAIN` na base de dev confirmar reordenação → `debug-specialist`, verificando os 5 chamadores antes de remover o `Distinct` |

## Deviation notes

- **Estado vazio (CA-09):** o `design.md` v1.0.0 não detalha o mecanismo. `TableContainer.emptyMessage`
  já aceita `ReactNode`, então é ligação, sem componente novo nem mudança de contrato — por isso fica
  registrado aqui e não gera nova versão do design.
- **Tupla no LINQ:** o design esboça `JoinCandidateAndJob` retornando `ValueTuple`. Se o Npgsql não
  traduzir composição posterior sobre a tupla, trocar por tipo anônimo ou classe privada de leitura —
  mesma semântica, sem mudança de contrato. Registrar aqui qual forma ficou.
  **Forma que ficou (backend, 2026-09-23):** classe privada de leitura
  `ApplicationWithJobAndCandidate { Application, Job?, Candidate? }` em `JobApplicationRepository`.
  A tupla nem chega a compilar: `select (application, job, candidate)` sobre `IQueryable` é literal
  de tupla em árvore de expressão (CS8143). Tipo anônimo não pode ser retornado pelo método. A classe
  com inicializador é o mesmo padrão de `JobWithCompany` em `JobRepository`, que já roda no Npgsql.
  Tradução em Postgres real **não verificada** nesta etapa (fixture in-memory) — fica para o E2E.
- **Ordenação sobre as linhas juntadas:** o design diz "projeção → `ApplyOrderBy`". A ordenação entra
  **antes** da projeção, sobre `ApplicationWithJobAndCandidate` (campos da candidatura): ordenar um
  record projetado por construtor não é traduzível. Cadeia real: `JoinCandidateAndJob` → `ApplySearch`
  → `ApplyOrderBy` → projeção → `ToPaginatedListAsync`. Mesmo resultado; antes a ordenação era
  aplicada à candidatura antes do join.
- **Controller (1.5):** o esboço declara `GetAll` com seis parâmetros sem `override`, o que deixaria
  duas actions `[HttpGet]` na mesma rota (a da base continua exposta). Seguido o precedente de
  `JobsController`: a override da base fica `[NonAction]` e delega à action nova com `status`.
- **Validator:** `GetAllJobApplicationsValidator` passou do genérico para `GetAllJobApplicationsQuery`.
  Os dois validators (geral e por vaga) ganharam `Search` com teto de 120 caracteres, o mesmo do feed
  (tarefa 2.3). Nenhum dos dois tem regra de `Status` — ver o item seguinte.
- **Status: fonte única, só por nome (revisão de código, 2026-09-23):**
  - (a) *Mudança observável no endpoint por vaga:* `status=99` passou de 200 com lista vazia para
    **400**; `status=Foo` continua 400, mas o código passou de `INVALID_PARAMS` para
    `INVALID_QUERY_FILTER`, como exige o design §1.2. O frontend não consome esses códigos.
  - (b) A validação de status saiu dos validators do FluentValidation (geral e por vaga) e ficou só no
    `ApplicationStatusParser`: todo valor inválido sai com `INVALID_QUERY_FILTER`. O validator de
    "minhas candidaturas" (`GetMyJobApplicationsQueryValidator`) ficou fora deste recorte e não mudou.
  - (c) Parse só por nome: `EnumNameParser.TryParseName<TEnum>` (em `Application/Utils/Helpers`)
    compara com os nomes declarados, sem distinção de caixa e com trim. Recusa número (`"1"`, `"99"`)
    e lista com vírgula (`"Approved,Pending"`, que o `Enum.TryParse` combinaria por OU bit a bit num
    terceiro membro). Também é usado pelo filtro `userType` de `GET /api/admin`.
- **`GetJobApplicationsByJobIdQuery.Search`** tem default `= null` (o design não tem): mantém as
  chamadas existentes compilando; o contrato HTTP é o mesmo.
- **Escopo de empresa:** mantido exatamente como estava (filtro por `job.CompanyId` sobre a query de
  candidaturas, antes do join de busca). Status é aplicado ali também, antes da busca e da paginação.
- **`useSearchParams` sob `cacheComponents`:** exige `<Suspense>` acima do componente cliente. Confirmar
  se o layout `(main)` já provê; se não, adicionar o boundary na página, não no hook.
  **Resolvido:** `src/app/(main)/layout.tsx` já envolve `MainLayout` em `<Suspense>` (acima do shell);
  nenhum boundary novo na página. `pnpm build` passou sem erro de prerender e as rotas mantiveram a
  classificação (`○` nas três listas, `◐` em `/recrutamento/vagas/[id]/candidatos`).
- **Ordem real de implementação:** esta feature foi a primeira; o hook nasceu aqui e a
  `emp-filtro-tipo-usuario-admin` o reaproveitou na mesma entrega.
- **Assinatura do hook (4.1):** `useUrlSyncedParams(defaults, schema)` — recebe também o schema Zod do
  formulário, porque a tarefa exige ignorar valor fora do enum e o esboço do design só tinha `defaults`.
  Cada valor da URL é validado trocando só aquele campo nos defaults e rodando o schema; o que falha cai
  no default sem afetar os outros campos. O retorno final ficou `{ values, resetKey, onChange, reset }`, em vez
  de `{ initialValues, onChange }` (ver o item "Revisão — URL mudada na mesma rota" abaixo).
- **Codec puro separado:** `parseUrlSyncedParams`, `serializeUrlSyncedParams` e `hasActiveUrlSyncedParams`
  ficam em `shared/hooks/url-synced-params-codec.ts` (sem React nem `next/navigation`), testáveis em Node;
  o hook só os liga ao router. `hasActiveUrlSyncedParams` usa a mesma regra da serialização e alimenta o
  estado vazio (5.4).
- **A URL guarda os valores do formulário, não os parâmetros da API:** os `FilterFields` passam a entregar,
  via `useFilterFormSync`, os **valores do formulário** (o design §4.2 dizia `onChange: (params)`); a página
  grava esses valores na URL e deriva os parâmetros da query com `*FilterToParams`. Guardar os parâmetros
  exigiria um mapeamento inverso por tela para voltar a `defaultValues` (ex.: `isDeleted` → `situation`).
  Novo `recruitmentApplicationsFilterToParams` no schema da tela de candidaturas.
- **Schema de candidatos por vaga extraído:** `candidatesFilterSchema`/`candidatesFilterToParams` saíram de
  `candidates-filter-fields.tsx` (`'use client'`, importa componentes e SCSS) para
  `candidates-filter-schema.ts`, para serem testáveis sem React. O `FilterBar` passou para dentro de
  `CandidatesFilterFields`, que agora hospeda o botão "Limpar", como nas outras telas.
- **"Limpar filtros" no estado vazio:** o estado vazio vive no `TableContainer`, fora do `FormProvider`, e
  o `FormProvider` não expõe `reset` para fora. A página chama `reset(defaults)` do hook, que troca os valores,
  grava a URL e incrementa `resetKey`; o formulário remonta por `key={resetKey}` e o `useFilterFormSync` não
  notifica os valores iniciais da remontagem, o que evita uma segunda requisição. (Na primeira versão isso
  dependia de um marcador `isFirstRun`, que só valia em produção — ver "Revisão do hook — StrictMode" abaixo.)
  Ordenação diferente do default também conta como "filtro ativo" (leitura literal da 5.4).
- **Sugestões da busca:** o `AutocompleteField` sugere nome, e-mail e título de vaga só da página carregada
  (mesmo comportamento das outras telas; o autocomplete no backend continua no *Adiado*).
- **Revisão — busca acima de 120 caracteres:** colar mais de 120 caracteres em Buscar passava pelo debounce
  (o `max(120)` do Zod não bloqueia o `watch`), a API respondia 400 pelo teto novo do validator e o
  `GracefullyDegradingErrorBoundary` trocava a tela pelo fallback. Correção na digitação:
  `AutocompleteInput`/`AutocompleteField` ganharam a prop opcional `maxLength`, repassada ao `<input>` do
  `CommandInput` (atributo nativo, sem mudar a semântica do campo) e aplicada também ao rótulo da sugestão
  escolhida, que não passa pelo `<input>`. Constante única `LIST_SEARCH_MAX_LENGTH` (+
  `LIST_SEARCH_MAX_LENGTH_MESSAGE`) em `shared/schema/list-params.ts`, usada no schema e no `maxLength` das
  quatro telas deste lote (candidaturas, candidatos por vaga, vagas, usuários). Uma busca maior vinda da
  URL já era recusada pelo schema e caía no default. `/admin/empresas` e `/recrutamento/candidatos`
  continuam com o literal 120 (fora deste escopo).
- **Revisão — URL mudada na mesma rota:** o App Router não remonta a página quando só a query muda (ex.:
  estando em `?status=Processing`, clicar no menu leva a `/recrutamento/candidaturas`), e com `initialValues`
  congelado no mount a URL ficava limpa com a lista ainda filtrada. O hook passou a guardar os valores
  correntes e a distinguir as queries que ele mesmo gravou (lista `pendingWrites`, descartada à medida que o
  router as devolve em `useSearchParams`) de uma mudança feita por fora. No segundo caso, relê a URL e
  incrementa `resetKey`; as páginas remontam o formulário com `key={resetKey}`, derivam a query dos `values`
  do hook (o `useState` local do filtro saiu) e voltam à página 1 quando `resetKey` muda. A releitura nunca
  chama `router.replace`, então não há ciclo. **Em estado, não em `useRef`** como sugerido: a comparação
  acontece durante o render, e o lint (`react-hooks/refs`, `react-hooks/set-state-in-effect`, do React
  Compiler) proíbe ler ref no render e dar `setState` em efeito; usado o padrão de ajuste de estado durante o
  render. Uma escrita é registrada contra a última pendente (e não só contra a URL), para "ir e voltar"
  antes de o router responder não parecer mudança externa.
- **Revisão do hook — eco casado com a escrita mais recente (`lastIndexOf`):** o eco era casado com
  `pendingWrites.indexOf(query)`. Sequência reproduzida pela revisão: URL limpa → Approved (pendentes `[A]`)
  → desfazer antes do eco (`[A, '']`) → o Next descarta a 1ª navegação e a 2ª termina em `''`, igual à query
  do estado, e as duas sobram → Approved de novo (`[A, '', A]`) → o eco `A` casava a posição 0 (`['', A]`)
  → o clique no menu para `''` casava `indexOf('')` e passava por eco: lista filtrada com URL limpa. Agora
  casa com a ocorrência mais recente e descarta ela e todas as anteriores (o eco mais novo invalida o que foi
  gravado antes dele).
- **Revisão do hook — StrictMode no `useFilterFormSync`:** a guarda `isFirstRun` se invertia com o
  double-invoke de efeitos do `StrictMode` (`reactStrictMode: true`): em dev, todo mount e toda remontagem por
  `key={resetKey}` notificava os valores iniciais → `setPage(1)` e um `router.replace` a mais (recarregar na
  página 3 com filtro ia para a 1). A guarda passou a comparar com a última chave notificada, iniciada com a
  do primeiro render. Em produção o comportamento não muda; conferidos os outros consumidores
  (`admin/empresas`, `recrutamento/candidatos`, `dashboard/analytics`): todos só dependem de "não notificar o
  valor inicial e notificar cada mudança de valor", que continua valendo.
- **Revisão do hook — `router.replace` só quando a query muda:** a escrita pulava a comparação e chamava
  `router.replace` mesmo com a query igual (ex.: espaço no fim da busca, que a serialização remove). Agora
  compara `nextQuery` com a última query conhecida (a última pendente, ou a do estado) e só troca a URL se
  for diferente; os valores do estado são atualizados de qualquer forma.
- **Revisão do hook — aviso de limite na busca (acessibilidade):** o `maxLength` nativo cortava em silêncio.
  O `AutocompleteInput` passou a mostrar "Limite de 120 caracteres." quando o texto atinge o limite, num `<p>`
  com `aria-live="polite"` sempre montado (leitor de tela só anuncia mudança em região que já existe),
  ligado ao `<input>` por `aria-describedby`; vazio, não ocupa altura (`.limitHint` sem padding fora do
  limite). O número vem da prop `maxLength`, que as quatro telas preenchem com `LIST_SEARCH_MAX_LENGTH`. O
  texto não reusa `LIST_SEARCH_MAX_LENGTH_MESSAGE` ("A busca não pode exceder…") porque o componente de `ui/`
  é genérico e não deve conhecer a regra de busca das listagens; a fonte única do 120 continua sendo a
  constante.
- **Revisão do hook — máquina de estados pura:** as regras saíram do hook para
  `shared/hooks/url-sync-state.ts` (sem React; exportadas também pelo barrel de `shared/hooks`):
  `createUrlSyncState(values, query)`, `lastKnownQuery(state)`, `reconcileQuery(state, query, readValues)` →
  `{ state, external }` (mesma referência de estado quando nada muda) e
  `registerWrite(state, values, nextQuery, remount = false)` → `{ state, shouldReplace }`. O hook só as
  orquestra; as escritas leem um espelho do estado em ref (atualizado na própria escrita e em
  `useLayoutEffect`), para duas escritas no mesmo evento não se perderem. `registerWrite` recebe também
  `values` e `remount` além de `nextQuery`, porque a escrita troca os valores e o `reset` remonta o formulário.
