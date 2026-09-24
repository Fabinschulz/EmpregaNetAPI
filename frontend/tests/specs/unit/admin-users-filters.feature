#language: pt

# Filtro "Tipo de usuário" em /admin/usuarios: o select mostra o rótulo pt-BR, mas o que vai para a
# API é o nome do enum (`UserTypeEnum`). O backend recusa o rótulo (`Recrutador`) com 400.
# Rastreio: emp-filtro-tipo-usuario-admin CA-01.
@emp-filtro-tipo-usuario-admin @CA-01
Funcionalidade: Filtro por tipo de usuário no backoffice de usuários
  Como administrador
  Quero filtrar a listagem de usuários por tipo
  Para achar recrutadores, gestores ou candidatos sem percorrer a lista inteira

  Cenário: CA-01 o filtro de Tipo oferece "Todos" seguido dos quatro tipos de usuário
    Então as opções de "userType" da tela "usuários do admin" devem ser "all" seguido de todos os tipos de usuário
    E devem existir 4 tipos de usuário para escolher

  Esquema do Cenário: CA-01 cada tipo aparece com rótulo em português e é aceito pelo formulário
    Então o tipo de usuário "<valor>" deve ser oferecido com o rótulo "<rotulo>"
    E o tipo de usuário "<valor>" deve ser aceito pelo filtro da tela "usuários do admin"

    Exemplos:
      | valor     | rotulo        |
      | Candidate | Candidato     |
      | Recruiter | Recrutador    |
      | Manager   | Gestor        |
      | Admin     | Administrador |

  Cenário: CA-01 "Todos" não vai para os parâmetros da API
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "userType" da listagem deve estar ausente
    E o parâmetro "isDeleted" da listagem deve estar ausente
    E o parâmetro "search" da listagem deve estar ausente

  Esquema do Cenário: CA-01 o tipo escolhido vai para a API pelo nome do enum, nunca pelo rótulo
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    E o campo "userType" do filtro vale "<valor>"
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "userType" da listagem deve ser "<valor>"
    E o parâmetro "userType" da listagem não deve ser o rótulo "<rotulo>"

    Exemplos:
      | valor     | rotulo        |
      | Candidate | Candidato     |
      | Recruiter | Recrutador    |
      | Manager   | Gestor        |
      | Admin     | Administrador |

  # O rótulo pt-BR e o `NaoSelecionado` nunca chegam à API: o formulário os recusa antes.
  Esquema do Cenário: CA-01 rótulo em português e tipo inexistente são recusados pelo formulário
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    E o campo "userType" do filtro vale "<valor>"
    Então o filtro não deve ser aceito pelo schema da tela

    Exemplos:
      | valor          |
      | Recrutador     |
      | Candidato      |
      | NaoSelecionado |
      | recruiter      |

  Cenário: CA-01 tipo, situação e busca seguem juntos para a API
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    E o campo "userType" do filtro vale "Manager"
    E o campo "situation" do filtro vale "deleted"
    E o campo "search" do filtro vale "  ana  "
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "userType" da listagem deve ser "Manager"
    E o parâmetro "isDeleted" da listagem deve ser o booleano "true"
    E o parâmetro "search" da listagem deve ser "ana"
    E o parâmetro "orderBy" da listagem deve ser "createdAt_DESC"

  Cenário: CA-01 tipo combinado com a situação "Ativos"
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    E o campo "userType" do filtro vale "Recruiter"
    E o campo "situation" do filtro vale "active"
    Quando eu converto o filtro em parâmetros da listagem
    Então o parâmetro "userType" da listagem deve ser "Recruiter"
    E o parâmetro "isDeleted" da listagem deve ser o booleano "false"
