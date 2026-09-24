---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Tasks — Filtro por tipo de usuário no backoffice de usuários

**feature-id:** `emp-filtro-tipo-usuario-admin` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.0.0 · **Spec:** [`spec.md`](spec.md) v1.0.0

**Ordem entre features:** implementar **depois** da `emp-filtros-candidaturas-recrutamento`, que cria
`shared/hooks/use-url-synced-params.ts`. Se a ordem se inverter, o Bloco 3 desta feature passa a
incluir a criação do hook (tarefas 4.1–4.2 daquela feature) e isso é registrado nas *Deviation notes*
das duas.

Restrição de release: **API antes ou junto do frontend** — com a API antiga, `userType` é ignorado e o
filtro aparece sem filtrar.

## Bloco 1 — Backend

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 1.1 | `GetAllUsersQuery` ganha `string? UserType = null` | Application | CA-02 |
| 1.2 | `GetAllUsersHandler`: após `EnsureAdministrator`, parse case-insensitive para `UserTypeEnum`, recusa `NaoSelecionado` e valor desconhecido com `INVALID_QUERY_FILTER`, aplica `Where(u => u.UserType == ...)` **antes** do `CountAsync` | Application | CA-02, CA-05 |
| 1.3 | `AdminController.GetAll`: `[FromQuery] string? userType = null` repassado ao query; atualizar o `<summary>` do endpoint | Api | CA-02 |

## Bloco 2 — Testes de backend

| # | Tarefa | CA |
| - | ------ | -- |
| 2.1 | Confirmar como o fixture de integração expõe `UserManager<User>`; ajustar o suporte de teste se necessário (em `backend/tests/Support`), sem mudar o handler | — |
| 2.2 | `Integration/Handlers/GetAllUsersHandlerIntegrationTests` (novo): filtro isolado e combinado com `isDeleted` e `search`; `totalItems` e paginação; valor desconhecido, `NaoSelecionado` e rótulo pt-BR recusados; caixa ignorada no nome do enum; não-admin barrado | CA-02, CA-05 |

## Bloco 3 — Frontend

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 3.1 | `shared/schema/list-params.ts`: `AdminUsersListQueryParams` ganha `userType?: string` | Frontend | CA-02 |
| 3.2 | `admin-users-filter-schema.ts`: `userType` (`'all'` + valores de `USER_TYPES`), default `'all'`; `adminUsersFilterToParams` omite `'all'` | Frontend | CA-01 |
| 3.3 | `AdminUsersFilterFields`: `SelectField "Tipo de usuário"` ao lado de "Situação", opções `Todos` + `USER_TYPE_OPTIONS`; o "Limpar" existente passa a cobrir o campo novo (vem de `defaultAdminUsersFilter`) | Frontend | CA-01 |
| 3.4 | `AdminUsersPage`: repassa `userType` à query, volta à página 1 ao trocar filtro, usa `useUrlSyncedParams` | Frontend | CA-02, CA-03 |
| 3.5 | Estado vazio com filtro ativo: `emptyMessage` com explicação + ação "Limpar filtros" | Frontend | CA-06 |
| 3.6 | `frontend/tests/specs/unit/admin-users-filters.feature` (novo): opções, `'all'` fora dos params, valor enviado é o nome do enum e não o rótulo | Testes | CA-01 |

## Bloco 4 — Verificação

| # | Tarefa | Quem | CA |
| - | ------ | ---- | -- |
| 4.1 | Revisão do diff (foco: filtro antes da contagem, guarda de admin preservada) | `code-reviewer` | — |
| 4.2 | E2E em `/admin/usuarios`: filtrar por cada tipo, combinar com situação e busca, recarregar, detalhe e voltar, estado vazio | `e2e-qa-skill` | CA-01, CA-02, CA-03, CA-05, CA-06 |
| 4.3 | Preencher o estado de cada linha da matriz do `spec.md` com evidência (CA-04 registrado como N/A) | Orquestrador | todos |

Gates antes do PR: `dotnet build` + `dotnet test` (com `-p:BaseOutputPath` se a API estiver rodando),
`pnpm lint`, `pnpm test`, `pnpm format:check` nos arquivos tocados.

## Adiado

| Item | Gatilho de retorno |
| ---- | ------------------ |
| Filtro por múltiplos tipos ao mesmo tempo | Pedido concreto de ver, por exemplo, "recrutadores e gestores" juntos |
| Filtro por empresa vinculada ao recrutador | Caso de uso de administração por empresa com utilizador identificado |
| Índice em `Users.UserType` | Latência de `GET /api/admin` aparecer como problema em métrica |
| Persistência de filtro na URL em `/admin/empresas` e `/recrutamento/candidatos` | Relato de perda de filtro ao navegar nessas telas — o hook já estará disponível |

## Deviation notes

- **Estado vazio (CA-06):** mesmo registro da `emp-filtros-candidaturas-recrutamento` — `emptyMessage`
  já aceita `ReactNode`; sem nova versão do design.
- **Ordem real de implementação:** depois da `emp-filtros-candidaturas-recrutamento`, na mesma entrega;
  `useUrlSyncedParams` reaproveitado sem mudança.
- **Mesmos desvios de mecanismo registrados lá:** o hook recebe o schema (`useUrlSyncedParams(defaults,
  schema)`); a URL guarda os valores do formulário (`?situation=deleted&userType=Recruiter`), e
  `AdminUsersFilterFields` passa a entregar esses valores via `useFilterFormSync`, com a página derivando os
  parâmetros por `adminUsersFilterToParams`; "Limpar filtros" no estado vazio remonta o formulário por `key`.
- **Ajustes da revisão (teto de 120 caracteres na busca; URL mudada na mesma rota sem remontar a página):**
  aplicados também a `/admin/usuarios`, que usa o mesmo hook e o mesmo `AutocompleteField` — assim como os
  da revisão focada do hook (`lastIndexOf`, StrictMode no `useFilterFormSync`, `router.replace` só quando a
  query muda, aviso de limite, máquina de estados pura). Ver os itens "Revisão" nas Deviation notes da
  [`emp-filtros-candidaturas-recrutamento`](../emp-filtros-candidaturas-recrutamento/tasks.md#deviation-notes).
- **Valores do enum:** `adminUsersUserTypeFilterValues` (`'all'` + `USER_TYPES[].value`) no schema, importado
  de `@/shared/utils/lib/user-types` (módulo puro) para o schema continuar testável em Node. `userType` na URL
  com rótulo pt-BR (`?userType=Recrutador`) é recusado pelo schema e cai em `'all'`.
- **Controller (1.3):** o esboço declara `public override ... GetAll(..., string? userType)`, que não
  compila (override exige a assinatura da base, de cinco parâmetros). Seguido o precedente de
  `JobsController`: a override da base fica `[NonAction]` e delega à action nova, que aceita `userType`.
  Rota, verbo, cache e policy inalterados.
- **Validação de `userType` só no handler:** não foi acrescentada regra no `GetAllUsersQueryValidator`
  — uma regra ali rejeitaria antes do handler, sem o código `INVALID_QUERY_FILTER` que o `spec.md`
  exige.
