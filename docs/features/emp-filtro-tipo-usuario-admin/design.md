---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Design técnico — Filtro por tipo de usuário no backoffice de usuários (`emp-filtro-tipo-usuario-admin`)

Solução para o [`prd.md`](prd.md) v1.0.0.

---

## 1. Levantamento

`GET /api/admin` já usa um record **dedicado** — `GetAllUsersQuery(Page, Size, OrderBy, IsDeleted,
Search)` (`AdminController.cs:39-48`, `GetAllUsersHandler.cs:17`) — e não o `GetAllQuery<T>` genérico.
Diferente do cenário de `emp-filtros-candidaturas-recrutamento`, aqui **não há conflito de tipo
genérico a resolver**: é só adicionar mais um campo opcional a um record que já é exclusivo deste
endpoint.

O tipo de usuário é uma coluna simples na própria tabela `Users`
(`User.UserType : UserTypeEnum`, `User.cs:23`), sem join necessário — não passa pelas tabelas de roles
do ASP.NET Identity (`AspNetRoles`/`AspNetUserRoles`), que existem só para sincronizar
`[Authorize(Policy=...)]` (`UserTypeRoleSync.cs:30-60`). Um filtro por tipo é um `WHERE` direto,
mesmo padrão dos filtros de `IsDeleted`/`Search` já existentes no mesmo handler.

`UserTypeEnum` (`EmpregaNet.Domain.Enums`) tem quatro valores relevantes:
`Candidate, Recruiter, Manager, Admin` (fora `NaoSelecionado`). O frontend já tem uma fonte única para
esses quatro valores com rótulo pt-BR — `USER_TYPE_OPTIONS`/`UserTypeValue` em
`shared/utils/lib/user-types.ts` — usada hoje na coluna "Tipo" da tabela e no formulário de edição de
usuário (`updateAdminUserRequestSchema`, que já valida `userType` contra esse mesmo conjunto). O
filtro reaproveita essa fonte tal como está, sem agrupar `Recruiter`/`Manager` nem inventar rótulo
novo — é o mesmo vocabulário que a tela já expõe na coluna Tipo.

---

## 2. Contrato — `GET /api/admin`

```csharp
// EmpregaNet.Application/Users/Queries/GetAllUsersHandler.cs
public sealed record GetAllUsersQuery(
    int Page, int Size, string? OrderBy, bool? IsDeleted = null, string? Search = null, string? UserType = null)
    : IRequest<ListDataPagination<UserViewModel>>, IPaginatedQuery;
```

Handler:

```csharp
if (!string.IsNullOrWhiteSpace(request.UserType))
{
    if (!Enum.TryParse<UserTypeEnum>(request.UserType, true, out var userType) ||
        userType == UserTypeEnum.NaoSelecionado)
    {
        throw new ValidationAppException(
            nameof(request.UserType),
            "Tipo de usuário inválido para filtro.",
            DomainErrorEnum.INVALID_QUERY_FILTER);
    }

    query = query.Where(u => u.UserType == userType);
}
```

Mesmo padrão de validação já usado para `Status` em
`GetJobApplicationsByJobIdHandler.ParseStatus` — `Enum.TryParse` case-insensitive, rejeita
`NaoSelecionado` como valor de filtro (não é um tipo real de usuário), erro de domínio existente
(`DomainErrorEnum.INVALID_QUERY_FILTER`), sem código de erro novo.

Controller (`AdminController.GetAll`):

```csharp
public override async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int size = 100,
    [FromQuery] string? orderBy = null,
    [FromQuery] bool? isDeleted = null,
    [FromQuery] string? search = null,
    [FromQuery] string? userType = null)
{
    var result = await _mediator.Send(new GetAllUsersQuery(page, size, orderBy, isDeleted, search, userType));
    return Ok(result);
}
```

Aditivo: parâmetro novo e opcional, nenhum dos existentes muda de nome, tipo ou posição observável
(query string não depende de ordem). **400** para `userType` desconhecido, mesmo formato de
`DomainError` já usado pelos demais filtros inválidos da API.

Sem migration, sem índice novo: `UserType` já é coluna da tabela `Users`; o volume de usuários do
projeto (ambiente de aprendizagem) não justifica um índice dedicado agora — mesmo raciocínio já
aplicado a `IsDeleted`/`Search` nesse handler, que também não têm índice próprio.

---

## 3. Frontend

### 3.1 Tipo de query

```ts
// shared/schema/list-params.ts
export type AdminUsersListQueryParams = ListQueryParams &
  Pick<SoftDeleteFilterParams, 'isDeleted'> &
  SearchFilterParams & { userType?: string };
```

### 3.2 Filtro

`admin-users-filter-schema.ts` ganha `userType: z.enum(['all', ...USER_TYPES.map((t) => t.value)])`
(mesmo padrão de `situation`/`status` já usados nos demais filtros administrativos), default `'all'`.

`AdminUsersFilterFields` ganha um `SelectField name="userType" label="Tipo de usuário"` com opções
`[{ label: 'Todos', value: 'all' }, ...USER_TYPE_OPTIONS]` (`USER_TYPE_OPTIONS` de
`@/shared/utils`, já existente — nenhum vocabulário novo), posicionado ao lado de "Situação" no mesmo
`FilterBar`. `adminUsersFilterToParams` passa a incluir `userType` (omitido do payload quando `'all'`,
mesmo tratamento que `situation` já recebe para `isDeleted`).

`AdminUsersPage`: `useAdminUsersQuery({ page, size, orderBy, isDeleted, search, userType })`.

### 3.3 Persistência de filtro na URL

Reaproveita o hook `useUrlSyncedParams` introduzido pela
[`emp-filtros-candidaturas-recrutamento`](../emp-filtros-candidaturas-recrutamento/design.md#45-persistência-de-filtro-na-url--hook-compartilhado)
— mesmo mecanismo, mesma composição com `useFilterFormSync`, sem hook novo. **Dependência de
sequenciamento:** se esta feature for implementada antes daquela, cabe a ela criar
`shared/hooks/use-url-synced-params.ts`; registrar a ordem real escolhida no `tasks.md`.

---

## 4. Riscos

| Risco | Mitigação |
| ----- | --------- |
| Usuário com múltiplos papéis simultâneos (se o domínio vier a permitir) | Hoje `UserType` é um valor único por usuário (`User.cs:23`), não uma coleção — CA-04 do PRD ("usuário aparece em todos os papéis que possuir") só se aplica se/quando o domínio mudar para múltiplos papéis; não há esse caso hoje, então o `WHERE` simples já satisfaz o critério tal como o domínio existe |
| `userType` inválido enviado manualmente na URL | Mesmo tratamento de 400 já usado pelos demais filtros de enum da API — não é caso novo a cobrir |
| Dependência do hook `useUrlSyncedParams` de outra feature | Ver §3.3 — resolvida por ordem de implementação, não por acoplamento de runtime entre features |
