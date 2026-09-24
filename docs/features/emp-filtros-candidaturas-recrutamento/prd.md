---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# PRD — Filtros e persistência nas telas de candidaturas do recrutamento (`emp-filtros-candidaturas-recrutamento`)

## 1. Problema e motivação

Três telas do fluxo de recrutamento cobrem volumes de dados que crescem continuamente (todas as
candidaturas que a empresa recebe, candidatos de uma vaga específica, vagas da empresa), mas os
controles para localizar um registro específico ficaram para trás:

1. **`/recrutamento/candidaturas`** — lista todas as candidaturas que a equipe de recrutamento
   enxerga. Hoje o único controle é ordenar por data. Não há filtro por status (mesmo sendo uma coluna
   proeminente da tabela) nem busca por candidato ou vaga. Achar "quem está pendente de triagem" ou "a
   candidatura de uma pessoa específica" exige ler a lista inteira, página a página.
2. **`/recrutamento/vagas/[id]/candidatos`** — já filtra por status, mas não tem busca textual. Em
   vagas populares, com centenas de candidaturas, restringir por status não é suficiente para achar um
   candidato específico dentro do subconjunto filtrado.
3. **`/recrutamento/vagas`** — gestão de vagas da empresa. É a única tela administrativa do produto
   sem controle de ordenação, mesmo o contrato de listagem já suportando esse parâmetro — inconsistência
   com as demais telas do mesmo perfil de usuário.

Um problema comum às três telas agrava tudo isso: o filtro aplicado **não sobrevive** a recarregar a
página nem a abrir o detalhe de um registro e voltar. O recrutador refaz a mesma busca repetidamente ao
longo do dia.

## 2. Personas e RBAC

| Persona | Acesso |
| ------- | ------ |
| Recrutador / Empresa | Vê e filtra apenas candidaturas, candidatos e vagas do próprio escopo (empresa), exatamente como hoje — esta feature não amplia nem restringe visibilidade. |
| Admin | Mesmo acesso que o recrutador nestas telas, sem mudança de RBAC. |

Nenhuma regra de autorização nova é introduzida: os filtros operam sobre o conjunto de dados que cada
perfil já está autorizado a ver.

## 3. Workflows

### W1 — Filtrar candidaturas por status

Em `/recrutamento/candidaturas`, o recrutador seleciona um status (ex.: "Em análise", "Aprovada",
"Rejeitada") ou "Todas". A lista passa a mostrar somente as candidaturas nesse status, combinando com
busca e ordenação.

### W2 — Buscar candidatura por candidato ou vaga

Na mesma tela, o recrutador digita nome/e-mail do candidato ou título da vaga; a lista se restringe aos
resultados correspondentes, combinando com o filtro de status ativo.

### W3 — Buscar candidato dentro de uma vaga

Em `/recrutamento/vagas/[id]/candidatos`, o recrutador digita nome/e-mail para restringir a lista já
filtrada por status.

### W4 — Ordenar vagas por data

Em `/recrutamento/vagas`, o recrutador escolhe ordenar por mais recentes ou mais antigas, no mesmo
padrão já usado pelas demais telas de gestão.

### W5 — Retomar filtro ao navegar

Nas três telas acima, recarregar a página, abrir o detalhe de uma candidatura/candidato/vaga e voltar,
ou reabrir a aba mais tarde, reproduz o mesmo status/busca/ordenação que o recrutador tinha aplicado.

## 4. Critérios de aceite

| # | Critério |
| - | -------- |
| CA-01 | Em `/recrutamento/candidaturas` existe um filtro de Status com todos os valores possíveis de candidatura, mais a opção "Todas". |
| CA-02 | Selecionar um Status na tela acima restringe a lista às candidaturas nesse status, combinando com busca e ordenação ativas. |
| CA-03 | Em `/recrutamento/candidaturas` existe um campo de busca textual que localiza candidaturas por nome/e-mail do candidato ou por título da vaga. |
| CA-04 | Em `/recrutamento/vagas/[id]/candidatos` existe um campo de busca textual por nome/e-mail do candidato, combinável com o filtro de Status já existente. |
| CA-05 | Em `/recrutamento/vagas` existe um controle de ordenação (mais recentes / mais antigas), no mesmo padrão das demais telas de gestão. |
| CA-06 | Nas três telas acima, recarregar a página preserva o status, a busca e a ordenação que estavam aplicados. |
| CA-07 | Nas três telas acima, abrir o detalhe de um registro (candidatura, candidato ou vaga) e voltar preserva o mesmo estado de filtro. |
| CA-08 | Cada uma das três telas mantém (ou ganha, onde ainda não existe) um controle "Limpar" que reseta filtro, busca e ordenação ao padrão da tela. |
| CA-09 | Quando nenhum registro atende aos critérios ativos, a tela explica a ausência de resultados e oferece limpar os filtros. |

## 5. Non-goals

Explicitamente **fora** desta entrega:

- **Filtro por vaga específica** dentro de `/recrutamento/candidaturas` — já existe uma tela dedicada
  (`/recrutamento/vagas/[id]/candidatos`) para navegar candidatos de uma vaga. Gatilho de retorno: se o
  volume de candidaturas por recrutador crescer a ponto de a busca textual por título de vaga não
  bastar para isolar uma vaga específica, revisitar.
- **Correção do comportamento do campo de busca** nas demais telas administrativas (empresas,
  usuários, candidatos, gestão de vagas), que hoje só sugere itens já carregados na página em vez de
  buscar no backend. É correção de um comportamento existente, não uma capability nova — tratar
  diretamente com o `debug-specialist`/implementação direta, fora do fluxo SDD.
- **Abas (Tabs) de status** substituindo o seletor — o seletor já cobre a segmentação necessária; abas
  seriam uma mudança de padrão visual sem ganho funcional.
- Exclusão de candidaturas/soft-delete (`isDeleted`) como filtro — caso de uso de manutenção, não de
  busca do dia a dia; sem utilizador identificado para esta entrega.

## 6. Dependências e restrições

- Assume que o endpoint de listagem geral de candidaturas (`/recrutamento/candidaturas`) aceita, ou
  pode aceitar sem quebra, parâmetros de status e de busca textual — a confirmar em `design.md`. Caso
  o contrato atual não suporte, a mudança de contrato HTTP entra no escopo do `design.md` (custo
  assimétrico aceito pelo processo SDD, decidido agora).
- A ordenação em `/recrutamento/vagas` depende apenas de expor no cliente um parâmetro (`orderBy`) que
  o tipo de listagem já declara — sem mudança de contrato esperada, a confirmar em `design.md`.
- A persistência de filtro na URL reaproveita, na medida do possível, o mesmo mecanismo já usado pelo
  feed de vagas (`emp-feed-vagas`) — não se desenha um mecanismo novo do zero.

## 7. Referências

- [`emp-feed-vagas/prd.md`](../emp-feed-vagas/prd.md) — precedente de persistência de filtros na URL
- `design.md`, `spec.md` — a seguir, após aprovação deste PRD
