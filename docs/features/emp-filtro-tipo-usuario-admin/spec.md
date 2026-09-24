---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Spec — Filtro por tipo de usuário no backoffice de usuários

**feature-id:** `emp-filtro-tipo-usuario-admin` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.0.0

Mapa de rastreio: cada critério de aceite do PRD tem um local de verificação. Contratos ficam no
`design.md`; passos de implementação, no `tasks.md`.

## 1. Matriz — critério de aceite → local de verificação

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-01 | Filtro de Tipo em `/admin/usuarios` com os tipos existentes + "Todos" | `frontend/tests/specs/unit/admin-users-filters.feature` (novo) — opções = `'all'` + os 4 valores de `USER_TYPE_OPTIONS` com rótulo pt-BR; `'all'` não vai para os parâmetros + E2E | BDD + E2E |
| CA-02 | Tipo restringe a lista, combinado com busca e situação | `backend/tests/Integration/Handlers/GetAllUsersHandlerIntegrationTests` (novo) — `userType` isolado; `userType` + `isDeleted`; `userType` + `search`; os três juntos | Integração |
| CA-03 | Recarregar ou voltar do detalhe preserva o tipo | `frontend/tests/specs/unit/url-synced-params.feature` (da `emp-filtros-candidaturas-recrutamento`, reaproveitado — sem cenário novo se o hook não mudar) + E2E: recarregar e usuário → detalhe → voltar | BDD + E2E |
| CA-04 | Usuário com múltiplos papéis aparece em cada tipo que possuir | **Não aplicável ao domínio atual:** `User.UserType` é valor único (`User.cs:23`), não coleção. Registrado como N/A com essa evidência; o comportamento de valor único é o que o CA-02 verifica | — |
| CA-05 | Contagem e paginação refletem só os usuários do filtro | `GetAllUsersHandlerIntegrationTests` — `totalItems` igual ao número de usuários do tipo; página 2 com `size` pequeno não traz usuário de outro tipo | Integração |
| CA-06 | Sem usuários do tipo, a tela explica e oferece limpar | E2E: combinação sem resultado (ex.: tipo + busca que não casa) → mensagem + ação de limpar que restaura a lista | E2E |

### Contrato e segurança (sem CA próprio, obrigatórios)

| Verificação | Onde |
| ----------- | ---- |
| `userType` desconhecido → `ValidationAppException` com `INVALID_QUERY_FILTER` (400) | `GetAllUsersHandlerIntegrationTests` |
| `userType=NaoSelecionado` recusado como filtro | `GetAllUsersHandlerIntegrationTests` |
| Aceita o nome do enum sem distinção de caixa (`recruiter`) e **não** aceita o rótulo pt-BR (`Recrutador`) — mesma regra de escrita já usada no `PUT /api/admin/{id}` | `GetAllUsersHandlerIntegrationTests` |
| Não-admin continua barrado: o handler segue chamando `AdministradorAccess.EnsureAdministrator` antes de aplicar qualquer filtro | `GetAllUsersHandlerIntegrationTests` — utilizador recrutador recebe a exceção de acesso, com e sem `userType` |

## 2. Cobertura por camada

| Camada | Alvos |
| ------ | ----- |
| Application | `GetAllUsersQuery.UserType` + parse/validação no handler |
| Api | `userType` novo e opcional em `GET /api/admin` |
| Frontend | `AdminUsersListQueryParams.userType`, schema e `SelectField` do filtro, `useUrlSyncedParams`, estado vazio com ação de limpar |

## 3. Gaps e riscos de cobertura

- **`UserManager.Users` no fixture:** o handler lê via `UserManager<User>.Users`. Se o fixture de
  integração existente não registrar Identity, o teste do CA-02/CA-05 precisa montar o `UserManager`
  sobre o contexto in-memory — a tarefa 2.1 confirma antes de escrever os cenários. Não se troca a
  leitura por repositório só para facilitar o teste.
- O `WHERE` em `UserType` é trivial no Npgsql; o E2E cobre a execução em Postgres real.
