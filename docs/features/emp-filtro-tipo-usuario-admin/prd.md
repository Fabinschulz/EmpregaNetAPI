---
version: 1.0.0
date: 2026-09-23
status: Approved
---

# PRD — Filtro por tipo de usuário no backoffice de usuários (`emp-filtro-tipo-usuario-admin`)

## 1. Problema e motivação

`/admin/usuarios` exibe o Tipo de usuário (candidato, recrutador, admin) como uma das colunas centrais
da tabela — é um dos atributos que mais frequentemente motiva o admin a procurar um registro (ex.:
"quero ver todos os recrutadores", "preciso auditar quem tem papel de admin"). Apesar disso, **não
existe filtro por tipo de usuário** nem na interface, nem, aparentemente, no contrato atual de listagem
consumido pelo cliente.

Numa base de usuários que cresce continuamente (candidatos, recrutadores e admins cadastrados ao longo
do tempo na mesma tabela), a única forma de responder a essas perguntas hoje é percorrer a listagem
inteira, página a página, lendo a coluna Tipo uma a uma — exatamente o cenário que a auditoria de UX
identificou como crítico: com centenas ou milhares de registros, o admin não consegue encontrar
rapidamente o que procura.

Diferente das demais lacunas identificadas na auditoria, esta provavelmente exige uma mudança de
contrato HTTP (o parâmetro de filtro por tipo precisa existir na API), não apenas reativar algo que já
existe na interface — por isso entra agora no fluxo SDD como um custo assimétrico deliberado, e não
como um adiamento.

## 2. Personas e RBAC

| Persona | Acesso |
| ------- | ------ |
| Admin | Único perfil com acesso a `/admin/usuarios`. Sem mudança de RBAC — a feature adiciona um filtro sobre o mesmo conjunto de usuários que o admin já está autorizado a ver e gerir. |

Nenhum outro perfil (candidato, recrutador) tem acesso a esta tela hoje, e isso não muda.

## 3. Workflows

### W1 — Filtrar por tipo de usuário

O admin seleciona um tipo (Candidato, Recrutador, Admin) ou "Todos" num controle de filtro. A lista
passa a mostrar somente usuários daquele tipo, combinando com a busca textual e o filtro de situação
(ativo/excluído) já existentes.

### W2 — Retomar filtro ao navegar

Recarregar a página, ou abrir o detalhe de um usuário e voltar, reproduz o mesmo tipo selecionado
anteriormente — mesmo princípio adotado pela `emp-filtros-candidaturas-recrutamento` para as telas de
recrutamento.

## 4. Critérios de aceite

| # | Critério |
| - | -------- |
| CA-01 | Existe um controle de filtro de Tipo de usuário em `/admin/usuarios`, com os tipos existentes na plataforma (Candidato, Recrutador, Admin) mais a opção "Todos". |
| CA-02 | Selecionar um tipo restringe a lista aos usuários daquele tipo, combinando com a busca e o filtro de situação (ativo/excluído) já existentes. |
| CA-03 | Recarregar a página, ou abrir o detalhe de um usuário e voltar, preserva o tipo selecionado. |
| CA-04 | Se o modelo de usuário permitir múltiplos papéis por conta, um usuário aparece no filtro de cada tipo correspondente a um papel que possua. |
| CA-05 | A contagem e a paginação da lista refletem apenas os usuários que atendem ao filtro de tipo ativo (e aos demais filtros combinados). |
| CA-06 | Sem nenhum usuário do tipo selecionado, a tela explica a ausência de resultados e oferece limpar os filtros. |

## 5. Non-goals

Explicitamente **fora** desta entrega:

- Edição do tipo/papel de um usuário a partir desta tela — já é fluxo do formulário de edição de
  usuário, sem mudança prevista aqui.
- Filtro por empresa vinculada ao recrutador — fora do escopo desta auditoria; sem critério de aceite
  que o justifique nesta entrega.
- Qualquer alteração ao modelo de papéis (`Role`) em si — a feature consome o que já existe (ADR 0005),
  não introduz nem altera papéis.

## 6. Dependências e restrições

- Requer confirmar em `design.md` se o endpoint atual (`GET /api/admin`) aceita hoje um parâmetro de
  tipo/papel. Se não aceitar, a extensão do contrato HTTP faz parte obrigatória do escopo desta
  feature — não é opcional nem adiável, dado o critério de "custos assimétricos" do processo SDD
  (contrato HTTP é decidido agora, não postergado).
- Depende do modelo de Identity (`User`/`Role`) já registrado no [ADR 0005](../../sdd/adrs/0005-identity-no-dominio.md)
  — esta feature não muda essa decisão, apenas expõe um filtro sobre ela.

## 7. Referências

- [ADR 0005](../../sdd/adrs/0005-identity-no-dominio.md) — Identity no domínio
- [`emp-filtros-candidaturas-recrutamento/prd.md`](../emp-filtros-candidaturas-recrutamento/prd.md) —
  precedente de persistência de filtro na URL em tela administrativa
- `design.md`, `spec.md` — a seguir, após aprovação deste PRD
