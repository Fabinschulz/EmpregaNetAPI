---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Tasks — Filtros de localização no feed de vagas

**feature-id:** `emp-filtros-vagas-localizacao` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.1.0 · **Spec:** [`spec.md`](spec.md) v1.0.0

Independente das outras duas features de filtros: não usa o hook de URL delas (o feed já tem o seu).
Pode ser implementada em paralelo.

O Bloco 1 (Estado) é entregável sozinho e resolve o item **Crítico** da auditoria sem tocar o backend.
Os Blocos 2–3 (Cidade) dependem de mudança de contrato e vêm depois.

## Bloco 1 — Estado (só frontend, entregável independente)

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 1.1 | Descomentar a seção "Estado" em `feed/filters/filters-form/index.tsx:142-156` e importar `UF_SELECT_OPTIONS`/`ufFullLabel` de `@/shared/schema` | Frontend | CA-01, CA-02 |
| 1.2 | `jobs-feed-filters.feature`: cenário de ida e volta `parse`/`serialize` com `uf` repetido (se ainda não coberto) | Testes | CA-07 |

## Bloco 2 — Contrato de cidades (backend)

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 2.1 | Record `VocabularyCityGroup(UF State, IReadOnlyList<string> Items)` no Domain, junto de `IJobRepository` | Domain | — |
| 2.2 | `IJobRepository.GetActiveCitiesByStateAsync` + implementação em `JobRepository` (ativas, não excluídas, cidade não vazia, `Trim`, `DISTINCT`, agrupado e ordenado) | Domain + Infra | CA-08 |
| 2.3 | `VocabularyCityGroupViewModel(string State, IReadOnlyList<string> Items)` e campo `Cities` em `JobVocabularyViewModel` | Application | CA-03 |
| 2.4 | `GetJobVocabularyHandler`: manter a parte estática em `static readonly`, injetar `IJobRepository`, compor `Cities` por requisição | Application | CA-03 |
| 2.5 | `Integration/Handlers/GetJobVocabularyHandlerIntegrationTests`: agrupamento por UF, distinct/trim, exclusão de inativa/excluída/sem cidade, UF sem vaga ausente | Testes | CA-08 |
| 2.6 | Rodar `Unit/Jobs/JobVocabularyTests` existente — deve seguir verde sem alteração | Testes | — |

## Bloco 3 — Cidade (frontend)

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 3.1 | `jobs-feed-response-schema.ts`: `vocabularyCityGroupResponseSchema` + `cities` com `.default([])`; `emptyJobVocabulary.cities = []` | Frontend | CA-03 |
| 3.2 | `feed/filters/city-groups.ts`: função pura `cityGroupsForStates` | Frontend | CA-04, CA-05 |
| 3.3 | Seção "Cidade" no `FeedFiltersForm`, logo após "Estado", com `GroupedCheckboxes` alimentado por `cityGroupsForStates` | Frontend | CA-03, CA-04, CA-05 |
| 3.4 | `jobs-feed-filters.feature`: cenários de `cityGroupsForStates` (sem UF, 1 UF, 2 UFs, UF sem vagas) + cobertura das 27 UFs em `ufFullLabel` + ida e volta de `city` | Testes | CA-04, CA-05, CA-07 |
| 3.5 | `job-vocabulary.feature`: parse com e sem `cities` | Testes | CA-03 |

## Bloco 4 — Verificação

| # | Tarefa | Quem | CA |
| - | ------ | ---- | -- |
| 4.1 | Revisão do diff | `code-reviewer` | — |
| 4.2 | E2E em `/vagas` contra a API de desenvolvimento (Postgres real): gaveta, Estado, Cidade dependente, chips, recarregar | `e2e-qa-skill` | CA-01…CA-07 |
| 4.3 | Preencher o estado de cada linha da matriz do `spec.md` (Verificado / Não verificado / Não implementado) com evidência | Orquestrador | todos |

Gates de qualidade antes do PR: `dotnet build` + `dotnet test` (com `-p:BaseOutputPath` se a API
estiver rodando), `pnpm lint`, `pnpm test`, `pnpm format:check` nos arquivos tocados.

## Adiado

| Item | Gatilho de retorno |
| ---- | ------------------ |
| Estado como pill na barra principal (hoje só na gaveta) | Dado de uso mostrando que o filtro de Estado é aplicado com frequência comparável às pills atuais |
| Normalização de grafia de cidades no cadastro de vaga | Aparecerem duplicatas de grafia visíveis no filtro de Cidade |
| Seletor de empresa no feed (`companyIds` já funciona por URL) | Existir uma página de empresa que precise listar as vagas dela |
| Corrigir `"Ceara"` → `"Ceará"` na `Description` do enum `UF` do backend | Fora desta feature — defeito de exibição independente, tratado à parte |
| Cidade qualificada pela UF no filtro (`cities[]` hoje leva só o nome: marcar "Bom Jesus" sob PI também traz as vagas de "Bom Jesus"/RS quando nenhum Estado está marcado) | Mudança de contrato do feed (`city` → par UF+cidade); retomar quando aparecer cidade homônima com vagas em duas UFs no catálogo real |
| Aparar espaços de cidades antigas (`UPDATE "Jobs" SET "City" = btrim("City")`): o vocabulário oferece o valor aparado, mas o feed compara com o valor gravado, e uma vaga antiga com `" Fortaleza "` não volta ao marcar "Fortaleza". Criação e edição já aparam (`JobFactory.cs:62`) | Encontrar no banco alguma vaga com `City <> btrim(City)`; é correção de dado pontual, não de código |

## Deviation notes

- Se o EF Core não traduzir `Distinct` sobre a projeção do owned type `Location` no Npgsql, materializar
  `(State, City)` com `Select` + `Distinct` em SQL simples e agrupar em memória — o resultado e o
  contrato não mudam; registrar aqui se acontecer.
  **Forma que ficou (backend, 2026-09-23):** já implementado na forma conservadora, sem esperar a
  falha. `GetActiveCitiesByStateAsync` faz `Where` (visibilidade do feed — ver abaixo —, UF ≠ `NaoSelecionado`,
  `City.Trim() != ""`) → `Select(new { State, City = City.Trim() })` → `Distinct` no banco (tipo
  anônimo de colunas escalares; `Trim` → `btrim`), e agrupa por UF e ordena em memória. UF em ordem
  do código (ordinal), cidades em ordem `InvariantCulture`. Tradução em Postgres real **não
  verificada** nesta etapa (fixture in-memory) — fica para o E2E (4.2).
- **Predicado de visibilidade alinhado ao feed:** as cidades partem de `VisibleCatalogJobs()`, o mesmo
  método privado de `JobRepository` de onde o feed agora parte (vaga ativa, não excluída e de empresa
  não excluída). Assim o filtro não oferece cidade cujas vagas o feed esconde (PRD: "cidades presentes
  no catálogo"). Decisão do coordenador, 2026-09-23.
- **UF `NaoSelecionado` excluída:** o design fala em "cidade vazia não entra"; vaga com UF
  `NaoSelecionado` também fica de fora, pelo mesmo motivo (risco "vagas sem `City`/`State`" do design,
  CA-08): não há Estado real a que associar a cidade, e o frontend não tem rótulo para esse código.
- **`VocabularyCityGroup`** ficou em `EmpregaNet.Domain/Common/`, ao lado de `JobFeedFilter` e
  `JobFeedProjection` (o design o justifica pelo mesmo motivo que `JobFeedFilter`), e não na pasta
  `Interfaces/`.
- **Tarefa 2.6:** `JobVocabularyTests` precisou de ajuste mínimo, não "sem alteração" — o handler agora
  recebe `IJobRepository` no construtor, e o fixture do teste usava `new()`. O fixture passa um mock que
  devolve lista vazia de cidades; nenhuma asserção mudou.
- **Seção "Cidade" sem opções (3.3, frontend):** `GroupedCheckboxes` devolve `null` com zero grupos, o que
  deixaria a seção abrir vazia (API ainda sem `cities`, nenhuma vaga com cidade, ou Estados marcados sem
  vagas). A seção mostra então um texto curto ("Nenhuma cidade com vagas abertas nos estados
  selecionados." / "... no momento."), com a classe `.sectionEmpty` em `filters.module.scss`. Sem
  componente novo.
- **Tipos exportados (frontend):** `city-groups.ts` exporta `CityGroupByState`/`CityGroupOption` além de
  `cityGroupsForStates`; `jobs-feed-response-schema.ts` exporta `VocabularyCityGroupResponse`.
