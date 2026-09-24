---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Spec — Filtros e persistência nas telas de candidaturas do recrutamento

**feature-id:** `emp-filtros-candidaturas-recrutamento` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.0.0

Mapa de rastreio: cada critério de aceite do PRD tem um local de verificação. Contratos ficam no
`design.md`; passos de implementação, no `tasks.md`.

## 1. Matriz — critério de aceite → local de verificação

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-01 | `/recrutamento/candidaturas` tem filtro de Status com todos os valores + "Todas" | `frontend/tests/specs/unit/recruitment-applications-filters.feature` (novo) — opções = `'all'` + `APPLICATION_STATUSES`; `'all'` não vai para os parâmetros + E2E | BDD + E2E |
| CA-02 | Status restringe a lista, combinado com busca e ordenação | `backend/tests/Unit/JobApplications/GetAllJobApplicationsHandlerTests` (existente, adaptado ao query dedicado: status repassado ao repositório; status inválido e `NaoSelecionado` → `ValidationAppException`) + `Integration/Handlers/GetAllJobApplicationsHandlerIntegrationTests` (novo: status + busca + escopo de empresa combinados) | Unitário + Integração |
| CA-03 | Busca por nome/e-mail do candidato ou título da vaga | `GetAllJobApplicationsHandlerIntegrationTests` — um cenário por campo (nome, e-mail, título), case-insensitive, e busca que não casa nada devolve `totalItems = 0` + E2E | Integração + E2E |
| CA-04 | Busca por candidato em `/recrutamento/vagas/[id]/candidatos`, combinável com Status | `Integration/Handlers/GetJobApplicationsByJobIdHandlerIntegrationTests` (existente, ampliado: busca isolada e busca + status) + `recruitment-applications-filters.feature` (`candidatesFilterToParams` inclui `search`) | Integração + BDD |
| CA-05 | Ordenação em `/recrutamento/vagas` | `recruitment-applications-filters.feature` (`jobsFilterToParams` inclui `orderBy`, default `createdAt_DESC`) + E2E (ordem da tabela inverte). Backend já honra `orderBy` — sem mudança, sem teste novo | BDD + E2E |
| CA-06 | Recarregar preserva status, busca e ordenação | `frontend/tests/specs/unit/url-synced-params.feature` (novo — ida e volta parse/serialize; defaults omitidos da URL; valor inválido na URL cai no default) + E2E: recarregar nas três telas | BDD + E2E |
| CA-07 | Abrir o detalhe e voltar preserva o filtro | E2E nas três telas (candidatura → perfil do candidato → voltar; vaga → editar → voltar) | E2E |
| CA-08 | "Limpar" nas três telas reseta filtro, busca e ordenação | E2E: nas três telas, "Limpar" volta aos defaults **e** limpa a query string | E2E |
| CA-09 | Sem resultados, a tela explica e oferece limpar | E2E: busca sem correspondência → mensagem + ação de limpar que restaura a lista | E2E |

### Regressão (sem CA próprio, obrigatória)

| Verificação | Onde |
| ----------- | ---- |
| "Minhas candidaturas" inalterada após o refactor de `ProjectWithCandidate` | `Integration/Handlers/GetMyJobApplicationsHandlerIntegrationTests` (existente, deve seguir verde sem alteração) |
| Escopo de empresa preservado: recrutador não enxerga candidatura de outra empresa mesmo buscando pelo nome exato | `GetAllJobApplicationsHandlerIntegrationTests` — cenário explícito de isolamento |
| Candidato excluído logicamente continua aparecendo (LEFT JOIN mantido) | `GetAllJobApplicationsHandlerIntegrationTests` |
| `status` inválido no endpoint geral devolve 400 com o mesmo `DomainError` do endpoint por vaga | `Unit/JobApplications/GetAllJobApplicationsHandlerTests` |

## 2. Cobertura por camada

| Camada | Alvos |
| ------ | ----- |
| Infra | `JoinCandidateAndJob` + `ApplySearch` antes da projeção; status em `GetAllWithCandidateAsync`; `search` em `GetByJobIdAsync` |
| Application | `GetAllJobApplicationsQuery` dedicado; parser de status compartilhado entre os dois handlers |
| Api | `status` novo em `GET /api/jobapplications`; `search` novo em `GET /api/jobapplications/job/{jobId}`; nenhum parâmetro existente muda |
| Frontend | Tipos de `list-params.ts`; três schemas/filter-fields; `useUrlSyncedParams`; estado vazio com ação de limpar |

## 3. Gaps e riscos de cobertura

- **Tradução SQL real da busca:** o fixture in-memory aceita qualquer LINQ; o Npgsql pode recusar
  composição sobre tupla/tipo anônimo juntado. O E2E (CA-03/CA-04) contra a API de desenvolvimento é
  a única verificação em Postgres real — sem ele, essas linhas ficam **Não verificado**.
- **Cache de saída (verificado, sem risco):** `EntityRead` e `AuthenticatedRead` variam por utilizador e
  pela query string completa (`CacheVaryByRules.QueryKeys = "*"`, `OutputCachePolicyBase.cs:26,129`) —
  `?status=`/`?search=` novos ganham entrada própria sem ajuste.
