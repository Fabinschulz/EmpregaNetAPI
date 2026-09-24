#language: pt

# Formulário de filtro → parâmetros da API nas três listagens do recrutamento. A página entrega a
# `*FilterToParams` os valores crus do formulário (sem passar pelo schema), e é isso que se exercita.
# Rastreio: emp-filtros-candidaturas-recrutamento CA-01, CA-04 e CA-05.
@emp-filtros-candidaturas-recrutamento
Funcionalidade: Filtros das listagens de candidaturas e vagas do recrutamento
  Como recrutador
  Quero filtrar candidaturas por status, buscar candidatos e ordenar vagas
  Para encontrar rápido quem preciso avaliar sem percorrer a lista inteira

  @CA-01
  Cenário: CA-01 o filtro de Status oferece "Todas" seguido de todos os status de candidatura
    Então as opções de "status" da tela "candidaturas do recrutamento" devem ser "all" seguido de todos os status de candidatura
    E todo status de candidatura deve ter rótulo em português

  @CA-01
  Esquema do Cenário: CA-01 status fora da lista é recusado pelo formulário
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "status" do filtro vale "<status>"
    Então o filtro não deve ser aceito pelo schema da tela

    Exemplos:
      | status         |
      | NaoSelecionado |
      | Aprovada       |
      | Todas          |

  @CA-01
  Cenário: CA-01 "Todas" não vai para os parâmetros da API
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "status" da listagem deve estar ausente
    E o parâmetro "search" da listagem deve estar ausente
    E o parâmetro "orderBy" da listagem deve ser "createdAt_DESC"

  @CA-01
  Esquema do Cenário: CA-01 o status escolhido vai para a API pelo nome do enum
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "status" do filtro vale "<status>"
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "status" da listagem deve ser "<status>"

    Exemplos:
      | status              |
      | Pending             |
      | Approved            |
      | CanceledByCandidate |

  @CA-01
  Cenário: CA-01 status, busca e ordenação seguem juntos para a API
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "status" do filtro vale "Rejected"
    E o campo "search" do filtro vale "  Operador de empilhadeira  "
    E o campo "orderBy" do filtro vale "createdAt_ASC"
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "status" da listagem deve ser "Rejected"
    E o parâmetro "search" da listagem deve ser "Operador de empilhadeira"
    E o parâmetro "orderBy" da listagem deve ser "createdAt_ASC"

  @CA-01
  Cenário: CA-01 busca só com espaços não vira parâmetro
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "search" do filtro vale "   "
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "search" da listagem deve estar ausente

  @CA-04
  Cenário: CA-04 candidatos da vaga oferecem o mesmo filtro de Status
    Então as opções de "status" da tela "candidatos da vaga" devem ser "all" seguido de todos os status de candidatura

  @CA-04
  Cenário: CA-04 a busca por candidato vai para a API sem os espaços das pontas
    Dado que o filtro da tela "candidatos da vaga" está nos valores padrão
    E o campo "search" do filtro vale "  maria@email.com  "
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "search" da listagem deve ser "maria@email.com"
    E o parâmetro "status" da listagem deve estar ausente

  @CA-04
  Cenário: CA-04 busca por candidato combinada com status
    Dado que o filtro da tela "candidatos da vaga" está nos valores padrão
    E o campo "status" do filtro vale "Processing"
    E o campo "search" do filtro vale "João"
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "status" da listagem deve ser "Processing"
    E o parâmetro "search" da listagem deve ser "João"

  @CA-04
  Cenário: CA-04 busca vazia nos candidatos da vaga não vira parâmetro
    Dado que o filtro da tela "candidatos da vaga" está nos valores padrão
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "search" da listagem deve estar ausente
    E o parâmetro "status" da listagem deve estar ausente

  @CA-05
  Cenário: CA-05 a listagem de vagas pede as mais recentes por padrão
    Dado que o filtro da tela "vagas do recrutamento" está nos valores padrão
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "orderBy" da listagem deve ser "createdAt_DESC"

  @CA-05
  Cenário: CA-05 a ordenação escolhida na listagem de vagas vai para a API
    Dado que o filtro da tela "vagas do recrutamento" está nos valores padrão
    E o campo "orderBy" do filtro vale "createdAt_ASC"
    Quando eu converto o filtro em parâmetros da listagem
    Então o filtro deve ser aceito pelo schema da tela
    E o parâmetro "orderBy" da listagem deve ser "createdAt_ASC"

  @CA-05
  Cenário: CA-05 ordenação fora da lista é recusada pelo formulário de vagas
    Dado que o filtro da tela "vagas do recrutamento" está nos valores padrão
    E o campo "orderBy" do filtro vale "title_ASC"
    Então o filtro não deve ser aceito pelo schema da tela
