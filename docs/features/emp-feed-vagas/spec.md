---
version: 1.0.0
date: 2026-08-02
---

# Spec — Feed de Vagas (`emp-feed-vagas`)

Rastreio dos critérios de aceite do [`prd.md`](prd.md) até o ponto de verificação. Contratos e
endpoints não se repetem aqui — estão em [`design.md`](design.md).

## Matriz de verificação

| CA | Verificação | Onde |
| -- | ----------- | ---- |
| CA-01, CA-02 | Projeção do feed carrega todos os campos do cartão; hierarquia visual | `GetJobsFeedHandlerTests` (campos) + revisão visual |
| CA-03 | Página seguinte carrega ao atingir a sentinela | Revisão manual + `useInfiniteScroll` |
| CA-04 | Skeleton com a forma do cartão durante o carregamento | Revisão visual |
| CA-05 | Busca só dispara após pausa | `jobs-feed-filters.feature` (debounce da serialização) |
| CA-06 | Busca por cargo, empresa, tecnologia, cidade, sem acento | `GetJobsFeedHandlerTests` (montagem do filtro) — **execução SQL não coberta**, ver Gaps |
| CA-07 | Interseção entre grupos de filtro | `GetJobsFeedHandlerTests` |
| CA-08 | Multi-valor aplica OR interno | `GetJobsFeedHandlerTests` |
| CA-09 | Chip removível reflete no feed | `jobs-feed-filters.feature` |
| CA-10, CA-11 | Estado sobrevive a reload e é reproduzível por URL | `jobs-feed-filters.feature` (parse ↔ serialize *round-trip*) |
| CA-12 | Mesmos campos em painel e gaveta | `feed-filters-form` é componente único — verificado por inspeção |
| CA-13 | Estado vazio explica e oferece limpar | Revisão visual |
| CA-14, CA-15 | Estado de candidatura por sessão | `GetJobFeedInteractionsHandlerTests` |
| CA-16 | Contagem de candidatos | `GetJobsFeedHandlerTests` |
| CA-17, CA-18 | Teclado, `role="feed"`, `aria-live` | Revisão manual de a11y |
| CA-19 | `prefers-reduced-motion` | Bloco global em `globals.scss` — verificado por inspeção |
| CA-20 | Primeira página no HTML inicial | `pnpm build` + classificação da rota |
| CA-21 | Backfill de localização | Revisão da migration antes do `database update` |

## Cobertura complementar

| Alvo | Tipo | Onde |
| ---- | ---- | ---- |
| Limites do validator (size, tamanho de arrays, `SalaryMin ≤ SalaryMax`) | Unit | `GetJobsFeedValidatorTests` |
| Tradução de `publishedWithin` → instante (`today` ≠ `h24`) | Unit | `GetJobsFeedHandlerTests` |
| Feed nunca expõe vaga inativa ou excluída, mesmo para staff | Unit | `GetJobsFeedHandlerTests` |
| Vocabulário: todo valor do enum tem rótulo pt-BR | Unit (front) | `job-vocabulary.feature` |
| Faixas salariais → `salaryMin`/`salaryMax` | Unit (front) | `job-vocabulary.feature` |

## Gaps de cobertura conhecidos

1. **Busca full-text e operadores de array não são exercitados em teste automatizado.** A suíte de
   integração usa `Microsoft.EntityFrameworkCore.InMemory`, que não implementa `tsvector`,
   `websearch_to_tsquery`, `ts_rank` nem o operador de sobreposição de `text[]`. O que os testes
   cobrem é a **montagem** do filtro no handler; a **execução** do SQL fica sem rede de segurança.
   Fechar esse gap exige Postgres real na suíte (Testcontainers), que não está no projeto — decisão
   consciente, não esquecimento.
2. **Infinite scroll e a11y** dependem de verificação manual: o frontend usa Cucumber sobre lógica
   pura e não tem Testing Library, Playwright ou Cypress instalados.
3. **`ts_rank`** não tem asserção de qualidade de ranking — só se verifica que a ordenação por
   relevância é solicitada quando há busca ativa.
