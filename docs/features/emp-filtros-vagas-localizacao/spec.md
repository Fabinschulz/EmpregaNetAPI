---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# Spec — Filtros de localização no feed de vagas

**feature-id:** `emp-filtros-vagas-localizacao` · **PRD:** [`prd.md`](prd.md) v1.0.0 · **Design:** [`design.md`](design.md) v1.1.0

Mapa de rastreio: cada critério de aceite do PRD tem um local de verificação. Contratos ficam no
`design.md`; passos de implementação, no `tasks.md`.

## 1. Matriz — critério de aceite → local de verificação

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-01 | Controle de Estado na gaveta "Todos os filtros", multisseleção | E2E em `/vagas`: abrir a gaveta, a seção "Estado" existe, marcar 2 UFs | E2E |
| CA-02 | Estado filtra o feed em interseção com os demais filtros | Filtro `state[]` no backend já coberto pela `emp-feed-vagas` (sem mudança de código aqui) + E2E: UF + Modalidade → todos os cartões respeitam os dois | E2E (+ cobertura existente) |
| CA-03 | Controle de Cidade na gaveta | E2E em `/vagas`: seção "Cidade" existe na gaveta e lista cidades de vagas reais | E2E |
| CA-04 | Com Estado selecionado, só cidades desse Estado | `frontend/tests/specs/unit/jobs-feed-filters.feature` — cenários de `cityGroupsForStates` (1 UF, 2 UFs, UF sem vagas) + E2E: marcar CE mostra só cidades do Ceará | BDD + E2E |
| CA-05 | Sem Estado, busca livre entre todas as cidades | `jobs-feed-filters.feature` — `cityGroupsForStates` sem UF devolve todos os grupos, rotulados por `ufFullLabel` + E2E: digitar parte do nome na busca da seção | BDD + E2E |
| CA-06 | Estado e Cidade como chips removíveis | E2E: aplicar UF + cidade → dois chips; remover o de cidade atualiza o feed e mantém o de UF | E2E |
| CA-07 | Estado e Cidade na URL, sobrevivem a recarregar e a outro navegador | `jobs-feed-filters.feature` — ida e volta `parse`/`serialize` com `uf` e `city` repetidos + E2E: recarregar mantém chips e resultados | BDD + E2E |
| CA-08 | Vagas sem localização visíveis sem filtro; fora só com filtro de localização | `backend/tests/Integration/Handlers/GetJobVocabularyHandlerIntegrationTests` (vaga inativa, excluída ou sem cidade **não** gera entrada em `Cities`) + comportamento do feed sem filtro inalterado (cobertura existente) | Integração |

### Contrato (sem CA próprio, mas condição do CA-03/04/05)

| Verificação | Onde |
| ----------- | ---- |
| `Cities` agrupa por código de UF, cidades distintas e ordenadas, UF sem vaga não aparece | `GetJobVocabularyHandlerIntegrationTests` |
| Duas vagas na mesma cidade geram uma entrada só; `" Fortaleza "` e `"Fortaleza"` colapsam | `GetJobVocabularyHandlerIntegrationTests` |
| Campos estáticos do vocabulário (enums, requisitos, benefícios) continuam idênticos | `backend/tests/Unit/Jobs/JobVocabularyTests` (existente, deve seguir verde) |
| Cliente aceita resposta **sem** `cities` (deploy fora de ordem) e com `cities` | `frontend/tests/specs/unit/job-vocabulary.feature` — dois cenários de parse |
| Todo código de `UF_SELECT_OPTIONS` tem rótulo em `ufFullLabel` | `jobs-feed-filters.feature` — um cenário cobrindo as 27 UFs (garante que `cityGroupsForStates` nunca rotula um grupo com código cru) |

## 2. Cobertura por camada

| Camada | Alvos |
| ------ | ----- |
| Infra | `JobRepository.GetActiveCitiesByStateAsync`: filtro ativo/não excluído/cidade não vazia, `DISTINCT`, agrupamento |
| Application | `GetJobVocabularyHandler`: parte estática intacta + `Cities` mapeado para código de UF |
| Api | Sem mudança de rota nem de cache; só o corpo da resposta ganha `cities` |
| Frontend | `cityGroupsForStates` (função pura), schema de vocabulário, seções Estado/Cidade no `FeedFiltersForm` |

## 3. Gaps e riscos de cobertura

- **Tradução SQL real:** os testes de integração usam o fixture in-memory. `Distinct` + `GroupBy` sobre
  owned type (`Location`) traduz de forma diferente no Npgsql. A verificação contra Postgres real é o
  E2E (CA-03/CA-04), que roda contra a API de desenvolvimento — se o E2E não rodar, CA-03 fica
  **Não verificado** e o fecho não pode sair com confiança alta.
- **Invalidação de cache após criar vaga em cidade nova:** depende da tag `Job` já existente. Não se
  escreve teste novo para o mecanismo de cache (não muda); o E2E pode confirmar criando uma vaga e
  recarregando o vocabulário, se o ambiente permitir — caso contrário, fica registrado como não
  verificado nesta execução.
