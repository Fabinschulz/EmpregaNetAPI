---
version: 1.1.0
date: 2026-09-23
status: Approved
---

# Design técnico — Filtros de localização no feed de vagas (`emp-filtros-vagas-localizacao`)

Solução para o [`prd.md`](prd.md) v1.0.0. Recorte pequeno e deliberadamente contido: Estado é
100% reativação de UI já implementada; Cidade é a única parte que toca o backend.

---

## 1. Levantamento (por que Estado é só front, e Cidade não é)

- **Estado (UF):** `JobsFeedFilters.states`, as chaves de URL (`uf`), a serialização
  (`jobsFeedFiltersToApiParams` → `state: string[]`) e o chip removível
  (`FeedActiveChips` → `addMany('states', ufFullLabel)`) já existem e funcionam. O único ponto
  que falta é o bloco de UI, comentado em
  `frontend/src/features/vagas/feed/filters/filters-form/index.tsx:142-156`. O `GET /api/jobs/feed`
  já aceita o parâmetro `state[]` (`JobsFeedQueryParams.state`) — nada no backend muda.
- **Cidade:** `JobsFeedFilters.cities` e a chave de URL (`city`) também já existem, mas **não há
  nenhuma fonte de opções de cidade** — ao contrário de Estado (enum fechado), cidade é texto livre
  que só existe espalhado nas linhas de `Jobs`. `JobVocabularyViewModel` hoje não tem esse campo.
  Populá-lo exige uma consulta `DISTINCT` sobre vagas ativas — é o único ponto de contrato desta
  feature.

## 2. Contrato — `GET /api/jobs/vocabulary`

Mudança **aditiva** (não quebra nenhum consumidor atual): `JobVocabularyViewModel` ganha um campo
novo, `Cities`, agrupado por UF:

```csharp
// EmpregaNet.Application/Jobs/ViewModel/JobVocabularyViewModel.cs
public sealed class JobVocabularyViewModel
{
    // ...campos existentes...
    public required IReadOnlyList<VocabularyCityGroupViewModel> Cities { get; init; } // novo
}

public sealed record VocabularyCityGroupViewModel(string State, IReadOnlyList<string> Items); // novo
```

- `State` é o **código** da UF (`UF.ToString()`, ex. `"CE"`) — o mesmo valor que o feed já usa em
  `state[]` e na chave de URL `uf`. O agrupamento casa com o filtro de Estado por código, nunca por
  rótulo (ver §3.4 para o porquê).
- `Items` são as cidades distintas encontradas entre vagas **ativas e não excluídas**, ordenadas
  alfabeticamente. Cidade vazia/só espaços não entra. UFs sem nenhuma vaga não geram grupo.
- Não há `Label` no grupo: o rótulo por extenso é responsabilidade do frontend (`ufFullLabel`), que
  já é a fonte única de rótulos de UF na interface.

`GetJobVocabularyHandler` deixa de ser puramente estático nessa parte: a porção derivada de enums e
constantes continua montada uma vez (campo `static readonly`, como hoje), e `Cities` passa a vir de
uma consulta por requisição — que só acontece quando o cache HTTP do endpoint expira ou é invalidado.
A consulta entra num método novo de `IJobRepository`:

```csharp
Task<IReadOnlyList<VocabularyCityGroup>> GetActiveCitiesByStateAsync(CancellationToken cancellationToken);
```

```csharp
// esboço — DISTINCT State, City sobre vagas ativas e não excluídas
var rows = await _context.Jobs.AsNoTracking()
    .Where(j => j.IsActive && !j.IsDeleted && j.Location.City != "")
    .Select(j => new { j.Location.State, j.Location.City })
    .Distinct()
    .ToListAsync(cancellationToken);

return rows
    .GroupBy(r => r.State)
    .OrderBy(g => g.Key.ToString())
    .Select(g => new VocabularyCityGroup(g.Key, g.Select(r => r.City.Trim()).Distinct().OrderBy(c => c).ToList()))
    .ToList();
```

`VocabularyCityGroup` (record de leitura com `UF State` tipado) vive no Domain, ao lado de
`IJobRepository`, pelo mesmo motivo que `JobFeedFilter` vive lá (o repositório precisa do tipo e o
Domain não depende da Application). O handler converte para `VocabularyCityGroupViewModel` com
`State.ToString()`.

**Cache:** o endpoint já usa `[OutputCache(PolicyName = OutputCachePolicies.PublicCatalog)]` com
invalidação por tag de entidade `Job` — a mesma tag que o feed e a listagem de vagas já invalidam a
cada criação/edição de vaga. Uma vaga nova numa cidade inédita aparece no vocabulário assim que essa
tag invalida o cache, sem mecanismo novo de invalidação.

Nenhuma migration, nenhuma coluna nova, nenhum índice novo: a consulta lê `Jobs."City"`/`"State"`
(colunas já existentes desde a `emp-feed-vagas`, com índice `IX_Jobs_Location (State, City)`) e roda
só quando o cache do vocabulário é recalculado — não a cada busca do feed.

## 3. Frontend

### 3.1 Reativar Estado

Descomentar o bloco em `filters-form/index.tsx:142-156`, com um único ajuste: `UF_SELECT_OPTIONS` e
`ufFullLabel` precisam ser importados em `filters-form/index.tsx` a partir de `@/shared/schema`
(`uf-schema.ts`). Nenhuma outra mudança nesse arquivo.

Fica **só na gaveta "Todos os filtros"**, no mesmo lugar em que já estava antes de ser comentado —
não vira pill. Promover a pill é uma escalada de design (mexe no espaço já apertado da barra de
pills) que o PRD não pede; reativar o que já existia resolve o CA-01 sem essa decisão extra.

### 3.2 Adicionar Cidade

Nova `FilterSection` em `filters-form/index.tsx`, logo abaixo da de Estado, usando o mesmo componente
`GroupedCheckboxes` já usado por Benefícios/Requisitos. A seleção dos grupos visíveis é uma função
pura, testável isoladamente:

```ts
// features/vagas/feed/filters/city-groups.ts
export function cityGroupsForStates(
  cities: readonly { state: string; items: string[] }[],
  selectedStates: readonly string[]
): { label: string; items: string[] }[] {
  const visible = selectedStates.length > 0 ? cities.filter((g) => selectedStates.includes(g.state)) : cities;
  return visible.map((g) => ({ label: ufFullLabel(g.state), items: g.items }));
}
```

```tsx
<FilterSection title="Cidade" activeCount={filters.cities.length} summary={listSummary(filters.cities, (c) => c)}>
  <GroupedCheckboxes
    legend="Cidade"
    legendHidden
    columns
    searchAfter={SEARCH_AFTER}
    groups={cityGroupsForStates(vocabulary.cities, filters.states)}
    selected={filters.cities}
    onToggle={toggle('cities')}
  />
</FilterSection>
```

- **CA-04** (cidades restritas ao Estado selecionado): `cityGroupsForStates` mantém só os grupos
  cujo código de UF está em `filters.states`.
- **CA-05** (sem Estado, busca livre): sem Estado selecionado, todos os grupos; a busca textual já é
  do próprio `GroupedCheckboxes` (`searchAfter`, o mesmo mecanismo de Benefícios/Requisitos).

Desmarcar um Estado **não** remove automaticamente as cidades já selecionadas dele: o chip da cidade
continua visível e removível, e a interseção com o Estado restante simplesmente devolve zero vagas
dessa cidade. Limpar em cascata seria efeito colateral escondido numa ação que o candidato não pediu.

Nenhuma mudança em `active-chips/index.tsx` (já tem `addMany('cities', (city) => city)`) nem em
`jobs-feed-filters.ts` (parse/serialize de `cities` já existem) nem em `filter-pills/index.tsx`
(Cidade também fica só na gaveta, mesmo raciocínio do §3.1).

### 3.3 Contrato de resposta no cliente

`jobVocabularyResponseSchema` (`jobs-feed-response-schema.ts`) ganha o campo aditivo:

```ts
const vocabularyCityGroupResponseSchema = z.object({
  state: z.string(),
  items: z.array(z.string())
});

export const jobVocabularyResponseSchema = z.object({
  requirements: z.array(vocabularyGroupResponseSchema),
  benefits: z.array(vocabularyGroupResponseSchema),
  cities: z.array(vocabularyCityGroupResponseSchema).default([]), // novo
  maxItemsPerJob: z.number().int().positive()
});
```

`.default([])` faz o frontend tolerar uma API ainda sem o campo (deploy fora de ordem) sem quebrar o
parse do vocabulário inteiro — Cidade apenas fica sem opções. `emptyJobVocabulary` ganha `cities: []`.

### 3.4 Por que agrupar por código e não por rótulo (decisão da v1.1.0)

A v1.0.0 propunha grupos com `Label` = `UF.ToDescription()` e o frontend casando esse texto com
`ufFullLabel(uf)`. A comparação dos 27 rótulos durante a fase de spec mostrou que eles **já divergem
hoje**: o backend descreve `CE` como `"Ceara"` e o frontend como `"Ceará"`. Com correspondência por
rótulo, o filtro de Cidade por Estado falharia em silêncio para o Ceará — e para qualquer divergência
futura. Casar por código (`"CE"`) elimina a dependência de texto entre as duas pontas; o rótulo passa
a vir de uma única fonte (o frontend).

A grafia `"Ceara"` no enum do backend é um defeito à parte (afeta outras telas que exibem a
descrição da UF) e fica fora desta feature.

---

## 4. Riscos

| Risco | Mitigação |
| ----- | --------- |
| Consulta `DISTINCT` de cidades cresce com o catálogo | Roda só em cache miss do vocabulário e usa `IX_Jobs_Location`; volume atual é baixo (ambiente de aprendizagem) |
| Mesma cidade grafada de formas diferentes em vagas distintas ("Sao Paulo" / "São Paulo") | Aparecem como duas opções; o filtro do feed já compara por igualdade, então cada opção devolve exatamente as vagas com aquela grafia. Normalizar cidades é outra feature (cadastro de vaga), não esta |
| Deploy do frontend antes da API | `cities` com `.default([])` (§3.3) — Cidade fica sem opções, nada quebra |
| Vagas sem `City`/`State` preenchidos | Não entram em nenhum grupo de `Cities` — coerente com CA-08 do PRD |

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 1.1.0 | `Cities` passa a agrupar por código de UF (`VocabularyCityGroupViewModel(State, Items)`) em vez de rótulo, após a divergência `"Ceara"`/`"Ceará"` encontrada entre back e front; extração de `cityGroupsForStates` como função pura; `.default([])` no schema do cliente; regra explícita de não limpar cidades em cascata ao desmarcar Estado |
| 1.0.0 | Versão inicial |
