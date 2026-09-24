---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# PRD — Filtros de localização no feed de vagas (`emp-filtros-vagas-localizacao`)

## 1. Problema e motivação

O feed público de vagas (`emp-feed-vagas`) já trata Estado (UF) e Cidade como filtros de primeira
classe: ambos têm campo no estado de filtros, são lidos e escritos na URL, e aparecem como chip
removível quando ativos. Na prática, porém, **não existe nenhum controle na interface** para o
candidato escolher um Estado ou uma Cidade — o único caminho para ativar esse filtro hoje é editar a
URL manualmente, algo que nenhum candidato faz.

Localização é, ao lado de cargo e modalidade de trabalho, um dos critérios mais usados por quem
procura emprego. Sem ele:

1. Um candidato que só pode trabalhar presencialmente numa cidade específica não tem como restringir
   o feed a essa cidade — precisa abrir vaga por vaga e checar o endereço, reproduzindo exatamente o
   problema de "um clique por vaga" que a `emp-feed-vagas` já resolveu para os demais critérios.
2. À medida que o catálogo de vagas cresce, a ausência deste filtro degrada a experiência
   proporcionalmente — o mesmo catálogo que hoje é navegável às cegas por localização deixa de ser.

## 2. Personas e RBAC

Sem mudança face à `emp-feed-vagas`: esta feature adiciona dois filtros ao mesmo conjunto de dados já
público, sem alterar quem vê o quê.

| Persona | Acesso |
| ------- | ------ |
| Visitante anónimo | Filtra por Estado/Cidade como qualquer outro filtro do feed |
| Candidato autenticado | Idem |
| Recrutador / Admin | Idem (o feed continua somente leitura para estes perfis) |

## 3. Workflows

### W1 — Filtrar por Estado

O candidato abre o controle de localização (junto às demais pills, ou na gaveta "Todos os filtros") e
seleciona um ou mais Estados. O feed passa a mostrar apenas vagas localizadas nesses Estados,
combinando com os demais filtros ativos.

### W2 — Filtrar por Cidade

O candidato abre o controle de Cidade, disponível na gaveta "Todos os filtros". Quando já houver
Estado(s) selecionado(s), as cidades oferecidas se restringem a esses Estados; sem Estado selecionado,
o campo aceita busca livre por texto entre as cidades presentes no catálogo de vagas.

### W3 — Combinar e persistir

Estado e Cidade se comportam como os demais filtros do feed: aparecem como chip removível, entram na
interseção com os outros critérios ativos, e ficam refletidos na URL — recarregar a página ou abrir o
link noutro navegador reproduz o mesmo conjunto de resultados.

## 4. Critérios de aceite

| # | Critério |
| - | -------- |
| CA-01 | Existe um controle de filtro de Estado (UF) na interface do feed, com o mesmo padrão visual dos demais filtros (pill e/ou gaveta "Todos os filtros"), permitindo selecionar um ou mais Estados. |
| CA-02 | Selecionar um Estado filtra o feed para vagas localizadas nesse Estado, respeitando a interseção com os demais filtros ativos. |
| CA-03 | Existe um controle de filtro de Cidade na gaveta "Todos os filtros". |
| CA-04 | Com pelo menos um Estado selecionado, as opções de Cidade oferecidas se restringem às cidades desse(s) Estado(s). |
| CA-05 | Sem Estado selecionado, o campo de Cidade aceita busca por texto livre entre as cidades presentes no catálogo. |
| CA-06 | Estado e Cidade aparecem como chip removível quando ativos, e a remoção do chip atualiza o feed, seguindo o padrão já usado pelos demais filtros. |
| CA-07 | Estado e Cidade continuam refletidos na URL: recarregar a página ou abrir o link noutro navegador reproduz o mesmo conjunto de resultados. |
| CA-08 | Vagas sem Estado/Cidade preenchidos (anteriores ao enriquecimento do agregado `Job`) continuam visíveis quando nenhum filtro de localização está ativo, e ficam de fora apenas quando o candidato filtra explicitamente por Estado ou Cidade. |

## 5. Non-goals

Explicitamente **fora** desta entrega:

- Geolocalização automática do candidato ("vagas perto de mim", filtro por raio de distância).
- Autocomplete de cidade contra uma base externa de municípios (ex.: IBGE) — a lista de cidades
  oferecida vem das vagas já cadastradas no catálogo.
- Qualquer mudança na modalidade de trabalho (remoto/híbrido/presencial) — já coberta pela
  `emp-feed-vagas` e fora do escopo desta feature.
- Novo campo de endereço na vaga — Estado e Cidade já existem no agregado `Job`.

## 6. Dependências e restrições

- Depende inteiramente do agregado `Job` já enriquecido pela `emp-feed-vagas` (ver ADR 0006) — os
  campos de Estado e Cidade já existem no contrato e já circulam na URL; esta feature é, em
  princípio, estritamente de interface, sem endpoint novo. Confirmar em `design.md`.
- Reaproveita o mecanismo de estado/URL já implementado pela `emp-feed-vagas` — não se desenha um
  mecanismo novo de persistência.

## 7. Referências

- [`emp-feed-vagas/prd.md`](../emp-feed-vagas/prd.md) — feature-mãe onde os campos de localização
  nasceram
- [ADR 0006](../../sdd/adrs/0006-agregado-job-enriquecido.md) — enriquecimento do agregado `Job`
- `design.md`, `spec.md` — a seguir, após aprovação deste PRD
