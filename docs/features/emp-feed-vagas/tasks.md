---
version: 1.0.0
date: 2026-08-02
---

# Tasks - Feed de Vagas (`emp-feed-vagas`)

Ordem de execução. Detalhe técnico em [`design.md`](design.md).

## 1. Domínio

- [x] `WorkModelEnum`, `SeniorityEnum`, `JobAreaEnum` com `[Description]` em pt-BR
- [x] `JobTypeEnum`: remover `Remote`, reservar o índice 8, adicionar `Clt = 9` e `Pj = 10`
- [x] `JobLocation` (value object)
- [x] `JobFeedSortEnum`
- [x] `Job`: campos novos, construtor e `UpdateJob` atualizados
- [x] `JobFeedFilter` e `JobFeedProjection` (records de leitura)
- [x] `IJobRepository.GetFeedAsync`

## 2. Infra

- [x] `JobConfiguration`: owned `Location`, `text[]`, shadow `SearchVector`, índices
- [x] `CompanyConfiguration`: índice trigram em `CompanyName`
- [x] `JobRepository.GetFeedAsync` - join, projeção, filtros, ordenação
- [x] Migration com extensões, configuração de busca, data-move e backfill - **gerada e revisada; `database update` ainda NÃO executado (gate humano)**

## 3. Application

- [x] `JobVocabulary` - listas curadas de tecnologias e benefícios
- [x] `GetJobsFeedQuery` + handler + validator
- [x] `JobFeedItemViewModel` (enums como nome, `publishedAt` em ISO-8601)
- [x] `GetJobFeedInteractionsQuery` + handler
- [x] `CreateJobCommand` / `UpdateJobCommand` / `JobFactory` / validators com os campos novos
- [x] `JobViewModel` aditivo

## 4. Api

- [x] `GET /api/jobs/feed` - `[AllowAnonymous]` + `OutputCachePolicies.PublicCatalog`
- [x] `GET /api/jobs/feed/interactions` - `[Authorize]`
- [x] XML docs em pt-BR

## 5. Frontend - contratos

- [x] `shared/schema/job-vocabulary.ts`
- [x] `features/vagas/service/` (api, schema, keys, queries, server)
- [x] `features/recrutamento/vagas/service` alinhado ao contrato de escrita novo

## 6. Frontend - feed

- [x] `useJobsFeedFilters` (URL como fonte de verdade)
- [x] `useInfiniteScroll` em `shared/hooks`
- [x] `job-card` e subcomponentes + skeleton
- [x] Busca, painel/gaveta de filtros, chips ativos, ordenação, estado vazio
- [x] `page.tsx` como Server Component + `generateMetadata` + `loading.tsx`

## 7. Frontend - telas dependentes

- [x] Detalhe da vaga com os campos novos
- [x] `job-form.tsx` estendido

## 8. Verificação

- [x] Unit tests dos handlers e do validator
- [x] Cenários Cucumber: filtros na URL e vocabulário
- [x] `dotnet build` + `dotnet test`
- [x] `pnpm lint` + `pnpm build` + `pnpm test`
- [x] Atualizar `docs/README.md` e o índice de ADRs

## Deviation notes

Divergências entre o desenho e o que foi implementado.

1. **`GetAppliedJobIdsAsync` foi para `IJobApplicationRepository`, não `IJobRepository`.** O design
   colocava-o no repositório de vagas. É a versão em lote de `ExistsAsync(jobId, userId)`, que já
   vivia no repositório de candidaturas - separá-los deixaria a mesma pergunta em dois lugares.

2. **A coluna gerada de busca saiu de `JobConfiguration` para `JobSearchVector`, aplicada só sob
   `Database.IsNpgsql()`.** Descoberto ao rodar a suíte: o provider `InMemory` não conhece
   `NpgsqlTsVector` e falhava ao construir o **modelo inteiro** - 33 testes de integração sem
   nenhuma relação com vagas quebraram. O mapeamento específico do Postgres passou a ser aplicado
   no `OnModelCreating` do contexto.

3. **A migration gerada pelo EF foi reescrita à mão.** O scaffold produziu
   `DropColumn("Salary")` + `AddColumn("SalaryMin")`, que **apagaria o salário de todas as vagas**.
   Trocado por `RenameColumn` + `AlterColumn`. O scaffold também absorveu o índice único
   `IX_Companies_Name` no novo índice trigram (GIN não pode ser `UNIQUE`), o que teria removido a
   unicidade do nome da empresa - corrigido na origem, usando a sobrecarga `HasIndex(expr, nome)`.

4. **`salaryDisclosed` virou um seletor (`salaryDisclosure`) no formulário, não um checkbox.** O
   design system não tem `CheckboxField`, e "Divulgar faixa salarial / A combinar" comunica melhor
   que uma caixa marcada. Evitou criar um componente novo só para isto.

5. **UF saiu de `features/admin/empresas/service` para `shared/schema/uf-schema.ts`.** Não estava
   previsto, mas o feed tornou-se o terceiro consumidor - o limiar que a frontend-skill define para
   promover um helper a compartilhado.

6. **`TimeProvider` foi introduzido na Application.** A janela "publicadas hoje" depende do instante
   atual; injetar o relógio mantém o cálculo testável sem esperar a virada do dia. Registado em
   `RegisterApplicationDependencies`.

7. **`fetchJobFeedInteractions` divide a consulta em lotes de 100.** O endpoint tem esse teto e o
   scroll infinito acumula ids sem limite. Truncar faria as vagas a partir da sexta página perderem
   o estado de candidatura em silêncio.

8. **Dois defeitos encontrados pelos próprios testes novos:** `normalize` do vocabulário era
   sensível a caixa (`?model=remote` era descartado) e `parseJobsFeedFilters` usava a verificação
   exata em vez da normalização. Ambos corrigidos.

9. **Fixtures de teste do formulário de vaga atualizados**, não afrouxados. O contrato de escrita
   mudou de verdade; manter os payloads antigos passando exigiria aceitar vaga sem localização.

10. **`array_to_string` não serve em coluna gerada - descoberto ao aplicar a migration.** O
    `design.md` original usava-o, e o Postgres recusou com
    `42P17: generation expression is not immutable`. A função nativa é `STABLE` por causa da
    assinatura `anyarray` (o tipo do elemento pode ter função de saída dependente de contexto).
    A migration passou a criar `empreganet_array_to_text(text[])`, um invólucro `IMMUTABLE`
    especializado para texto - onde a operação é genuinamente imutável. A coluna gerada depende
    dessa função; removê-la quebra a tabela.

## Verificação contra o banco real

Depois de aplicada a migration, foi corrida uma sonda descartável chamando `GetFeedAsync` contra o
Postgres de desenvolvimento. Fecha parcialmente o gap 1 de [`spec.md`](spec.md): confirma que o
LINQ **traduz** - o que a suíte `InMemory` não consegue verificar.

| Verificado | Resultado |
| ---------- | --------- |
| 16 combinações de filtro/ordenação executam sem erro de tradução | sim |
| `EF.Property<NpgsqlTsVector>(...).Matches(...)` e `.Rank(...)` | traduzem |
| Sobreposição de arrays (`Technologies.Any(t => selecionadas.Contains(t))`) | traduz |
| Busca insensível a acento nos dois sentidos (`almoco`↔`almoço`) | sim |
| Pesos: título supera descrição no `ts_rank` | sim |
| Data-move: salários preservados, cidade/UF herdadas da empresa | sim |
| `IX_Companies_Name` (único) preservado ao lado do trigram | sim |

**Continua sem cobertura automatizada.** A sonda foi descartada; nada disto roda em CI.
