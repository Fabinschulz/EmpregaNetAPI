---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Design técnico — Filtros e persistência nas telas de candidaturas do recrutamento (`emp-filtros-candidaturas-recrutamento`)

Solução para o [`prd.md`](prd.md) v1.0.0. Investigação de contrato feita por leitura direta do
backend (não suposição) — evidência de arquivo:linha em cada seção.

---

## 1. `GET /api/jobapplications` (todas as candidaturas) — Status e busca

### 1.1 Estado atual

O endpoint usa hoje o `GetAllQuery<JobApplicationViewModel>` **genérico**, compartilhado por outras
entidades via `MainController.GetAll`
(`JobApplicationsController.cs:48-54` → `GetAllJobApplicationsHandler.cs:10,26-48`). O genérico
(`BaseQuery`) não tem campo `Status`, e o `Search` que o controller já aceita e repassa
(`MainController.cs:49`) é **ignorado pelo handler** (`GetAllJobApplicationsHandler.cs:34-39` não usa
`request.Search`).

**Decisão:** não estender o `GetAllQuery<T>` genérico — ele é usado por Jobs e outras entidades sem
noção de "status", e um campo `Status` ali vazaria para consumidores que não têm esse conceito. O
precedente correto já existe no próprio código: `GetAllUsersQuery` (Admin) e
`GetJobApplicationsByJobIdQuery` (candidatos por vaga) são **records dedicados**, não instâncias do
genérico. Este endpoint segue o mesmo caminho.

### 1.2 Contrato novo

```csharp
// EmpregaNet.Application/JobApplications/Queries/GetAllJobApplicationsQuery.cs (novo, substitui o uso do genérico aqui)
public sealed record GetAllJobApplicationsQuery(
    int Page, int Size, string? OrderBy, bool? IsDeleted, string? Search, string? Status)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;
```

`GetAllJobApplicationsHandler` passa a implementar
`IRequestHandler<GetAllJobApplicationsQuery, ListDataPagination<JobApplicationViewModel>>` (troca o
tipo genérico pelo dedicado). Validação do `Status` reaproveita literalmente o padrão já usado em
`GetJobApplicationsByJobIdHandler.ParseStatus` (`GetJobApplicationsByJobIdHandler.cs:64-81`) — mesmo
`Enum.TryParse<ApplicationStatusEnum>`, mesmo `DomainErrorEnum.INVALID_QUERY_FILTER` em caso de valor
inválido. Extrair esse parser para um helper compartilhado (`ApplicationStatusParser`) evita duplicar
a lógica entre os dois handlers — pequeno refactor, sem mudança de comportamento.

### 1.3 Controller

`JobApplicationsController.GetAll` deixa de chamar `base.GetAll(...)` (que monta o `GetAllQuery<T>`
genérico) e passa a montar o dedicado diretamente, no mesmo padrão já usado por `AdminController.GetAll`
(`AdminController.cs:39-48`):

```csharp
[HttpGet]
[OutputCache(PolicyName = OutputCachePolicies.EntityRead)]
[Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int size = 100,
    [FromQuery] string? orderBy = null,
    [FromQuery] bool? isDeleted = null,
    [FromQuery] string? search = null,
    [FromQuery] string? status = null)
{
    var result = await _mediator.Send(new GetAllJobApplicationsQuery(page, size, orderBy, isDeleted, search, status));
    return Ok(result);
}
```

`status` inválido → **400** (`ValidationAppException`, mesmo formato de erro já usado pelo endpoint
irmão). Sem mudança de rota, verbo, nem nos parâmetros já existentes — é aditivo.

### 1.4 Repositório — status e busca antes da projeção

`IJobApplicationRepository.GetAllWithCandidateAsync` ganha dois parâmetros opcionais:

```csharp
Task<ListDataPagination<JobApplicationProjection>> GetAllWithCandidateAsync(
    CancellationToken cancellationToken,
    int page,
    int size,
    string? orderBy = null,
    long? companyId = null,
    ApplicationStatusEnum? status = null,   // novo
    string? search = null);                 // novo
```

`status`: mesmo `if (status.HasValue) query = query.Where(a => a.Status == status.Value);` já usado em
`GetByJobIdAsync`/`GetByUserIdAsync` (`JobApplicationRepository.cs:171-174,192-195`) — nenhuma
novidade de padrão.

`search`: hoje o join com `Job`/`User` só existe **dentro** de `ProjectWithCandidate`
(`JobApplicationRepository.cs:213-235`), depois da paginação ser decidida. Para filtrar por
nome/e-mail do candidato ou título da vaga, o `Where` de busca precisa acontecer **antes** do
`ToPaginatedListAsync` — senão pagina sobre o conjunto errado. Extrai-se um passo intermediário que
expõe as tabelas já juntadas, e o filtro de busca entra ali:

```csharp
private IQueryable<(JobApplication Application, Job? Job, User? Candidate)> JoinCandidateAndJob(
    IQueryable<JobApplication> applications)
{
    return from application in applications
           join job in _context.Jobs.AsNoTracking() on application.JobId equals job.Id into jobs
           from job in jobs.DefaultIfEmpty()
           join user in _context.Users.AsNoTracking() on application.UserId equals user.Id into candidates
           from candidate in candidates.DefaultIfEmpty()
           select (application, job, candidate);
}

private static IQueryable<(JobApplication Application, Job? Job, User? Candidate)> ApplySearch(
    IQueryable<(JobApplication Application, Job? Job, User? Candidate)> query, string? search)
{
    if (string.IsNullOrWhiteSpace(search)) return query;
    var term = search.Trim().ToLower();
    return query.Where(x =>
        (x.Job != null && x.Job.Title.ToLower().Contains(term)) ||
        (x.Candidate != null && x.Candidate.UserName != null && x.Candidate.UserName.ToLower().Contains(term)) ||
        (x.Candidate != null && x.Candidate.Email != null && x.Candidate.Email.ToLower().Contains(term)));
}
```

`ProjectWithCandidate` deixa de fazer o join do zero e passa a receber o resultado de
`JoinCandidateAndJob` (já filtrado por `ApplySearch` quando aplicável) e só monta o `select new
JobApplicationProjection(...)`. `GetAllWithCandidateAsync` e `GetByJobIdAsync` passam a:
`JoinCandidateAndJob(query)` → `ApplySearch(..., search)` → projeção → `ApplyOrderBy` →
`ToPaginatedListAsync`. `GetByUserIdAsync` ("minhas candidaturas") chama a mesma cadeia com
`search: null` — comportamento inalterado, sem duplicar a query.

Nenhum join novo é introduzido (os mesmos `Jobs`/`Users` já eram consultados); o que muda é **onde**,
na cadeia de composição, o `Where` de busca entra.

---

## 2. `GET /api/jobapplications/job/{jobId}` (candidatos por vaga) — Busca

Já aceita e valida `status` (`GetJobApplicationsByJobIdQuery`, `GetJobApplicationsByJobIdHandler.cs:12,64-81`,
`JobApplicationRepository.cs:171-174`) — **sem mudança**. Ganha `search`, mesmo mecanismo do §1.4:

```csharp
public sealed record GetJobApplicationsByJobIdQuery(long JobId, int Page, int Size, string? Status, string? OrderBy, string? Search)
```

`GetByJobIdAsync` recebe `string? search = null` e aplica `JoinCandidateAndJob` + `ApplySearch` antes
da projeção, igual ao §1.4. Controller (`JobApplicationsController.GetByJob`) ganha
`[FromQuery] string? search = null` e repassa. Como `JobId` já vem fixo pela rota, a busca aqui é, na
prática, só sobre o candidato — mas reaproveitar `ApplySearch` (que também olha `Job.Title`) não custa
nada e mantém uma única implementação.

---

## 3. `GET /api/jobs` (gestão de vagas) — Ordenação

**Sem mudança de contrato.** `orderBy` já é aceito e honrado de ponta a ponta
(`JobsController.cs:42-52` → `GetAllJobHandler.cs:44-51` → `JobRepository.cs:67,250-262`, switch com
`createdAt_ASC/DESC`, `updatedAt_ASC/DESC`, `id_ASC/DESC`). O trabalho aqui é só de frontend (§4.3).

---

## 4. Frontend

### 4.1 Tipos de query (`shared/schema/list-params.ts`)

```ts
export type JobApplicationsListQueryParams = ListQueryParams & StatusFilterParams & SearchFilterParams;

export type JobApplicationsAdminListQueryParams = ListQueryParams &
  SoftDeleteFilterParams &
  StatusFilterParams &
  SearchFilterParams;
```

(Hoje `JobApplicationsListQueryParams` não tem `SearchFilterParams`, e
`JobApplicationsAdminListQueryParams` não tem nem `StatusFilterParams` nem `SearchFilterParams` — ver
`list-params.ts:52,54`.)

### 4.2 `/recrutamento/candidaturas`

- `recruitment-applications-filter-schema.ts` ganha `status` (`z.enum(['all', ...APPLICATION_STATUSES])`,
  mesmo padrão de `candidatesFilterSchema` em `candidates-filter-fields.tsx:10-14`) e `search`
  (`z.string()`).
- `RecruitmentApplicationsFilterFields` ganha `SelectField name="status"` (opções: "Todas" +
  `APPLICATION_STATUSES`/`applicationStatusLabels`, de `@/features/candidaturas/domain`, mesmo léxico
  já usado em `CandidatesFilterFields`) e `AutocompleteField name="search"` (placeholder "Candidato,
  e-mail ou vaga"), dentro de um `FilterBar` com o botão "Limpar" — mesmo padrão de
  `JobsFilterFields`/`AdminUsersFilterFields`. Passa a receber `onChange: (params) => void` em vez do
  atual `onChange: (orderBy) => void`, via `useFilterFormSync` (o hook já usado pelos outros três
  formulários administrativos) em vez do `useEffect` manual que o componente tem hoje
  (`recruitment-applications-filter-fields.tsx:18-25` — duplicava exatamente o que `useFilterFormSync`
  já resolve, ver comentário do próprio hook).
- `RecruitmentApplicationsPage`: `useAllJobApplicationsQuery({ page, size, orderBy, status, search })`.

### 4.3 `/recrutamento/vagas/[id]/candidatos`

`CandidatesFilterFields` ganha `AutocompleteField name="search"` (placeholder "Nome ou e-mail do
candidato"), ao lado de Status/Ordenar por já existentes. `candidatesFilterSchema` ganha `search:
z.string()`; `candidatesFilterToParams` passa a incluir `search`. `useApplicationsByJobQuery(jobId, {
page, size, status, orderBy, search })`.

### 4.4 `/recrutamento/vagas`

`jobs-filter-schema.ts` ganha `orderBy` (`z.enum(LIST_ORDER_BY_VALUES)`, default `'createdAt_DESC'`).
`JobsFilterFields` ganha `SelectField name="orderBy"` com `DATE_ORDER_BY_OPTIONS` (mesmo componente já
usado em `AdminUsersFilterFields`/`RecruitmentApplicationsFilterFields`). `RecruitmentJobsPage` passa
`orderBy` para `useJobsQuery`.

### 4.5 Persistência de filtro na URL — hook compartilhado

Hoje cada uma das três páginas guarda os parâmetros derivados em `useState` local
(`RecruitmentApplicationsPage`, por exemplo, guarda `orderBy` em `useState` — `index.tsx:57`) e só a
paginação (`page`/`pageSize`) persiste, via `usePersistedTablePagination` em `localStorage`. O feed de
vagas já resolveu o mesmo problema com a URL como fonte de verdade
(`use-jobs-feed-filters.ts`), mas naquele caso o formulário de filtro não é controlado por
React Hook Form — aqui é (`FormProvider` + `FilterFields` + `useFilterFormSync`), então o mecanismo
não pode ser copiado 1:1; precisa de uma variante que trabalhe **em cima** do
`onChange` que `useFilterFormSync` já entrega.

Novo hook em `shared/hooks/use-url-synced-params.ts`:

```ts
export function useUrlSyncedParams<TParams extends Record<string, unknown>>(
  defaults: TParams
): {
  initialValues: TParams;              // lido da URL no mount, com fallback em `defaults`
  onChange: (params: TParams) => void; // grava em `router.replace`, mesmo padrão de use-jobs-feed-filters
} {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const initialValues = useMemo(() => parseFromSearchParams(defaults, searchParams), [searchParams]);

  const onChange = useCallback(
    (params: TParams) => {
      const qs = paramsToSearchParams(params, defaults).toString();
      router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false });
    },
    [pathname, router]
  );

  return { initialValues, onChange };
}
```

`parseFromSearchParams`/`paramsToSearchParams` fazem a serialização genérica campo a campo (string,
enum, boolean) a partir do shape de `defaults` — sem parser dedicado por tela, ao contrário do feed
(que tem vocabulário rico o bastante para justificar um parser próprio). Cada página usa assim:

```ts
const { initialValues, onChange: onUrlChange } = useUrlSyncedParams(defaultRecruitmentApplicationsFilter);
// FormProvider defaultValues={initialValues}
// RecruitmentApplicationsFilterFields onChange={(params) => { onUrlChange(params); handleParamsChange(params); }}
```

O `onChange` do `FilterFields` passa a fazer duas coisas: atualizar o estado local que alimenta a
query (como já faz hoje) e escrever na URL (novo). A paginação continua em `localStorage` via
`usePersistedTablePagination`, sem mudança — só filtro/busca/ordenação migram para a URL, que é
exatamente o que os critérios de aceite pedem (CA-06/CA-07).

Este hook é introduzido por esta feature e fica em `shared/hooks/`, disponível para a
`emp-filtro-tipo-usuario-admin` reaproveitar sem reescrever o mecanismo.

---

## 5. Riscos

| Risco | Mitigação |
| ----- | --------- |
| `GetAllJobApplicationsHandler` muda de tipo de request (`GetAllQuery<T>` → dedicado) | Mudança interna ao Application; contrato HTTP do endpoint não muda nenhum parâmetro existente, só adiciona `status` |
| Busca por `Contains` sem índice dedicado em `Job.Title`/`User.UserName`/`User.Email` | Mesmo padrão já usado em `GetAllUsersHandler`/`JobRepository` hoje, sem índice; volume atual (ambiente de aprendizagem) não justifica um `pg_trgm` agora — revisitar se a métrica de latência do endpoint indicar necessidade |
| Refactor de `ProjectWithCandidate` para expor o join antes do `select` | Comportamento de `GetByUserIdAsync` ("minhas candidaturas") preservado por teste de regressão — `search: null` deve produzir exatamente o mesmo resultado de antes do refactor |
| Novo hook de URL usado por duas features | Esta feature o introduz; se `emp-filtro-tipo-usuario-admin` for implementada antes, cabe a ela criar o hook e esta feature reaproveita — registrar a ordem real de implementação no `tasks.md` de quem for a segunda |
