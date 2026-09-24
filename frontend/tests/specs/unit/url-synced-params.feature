#language: pt

# Codec puro de `useUrlSyncedParams` (`shared/hooks/url-synced-params-codec.ts`). A URL guarda os
# valores do formulário de filtro, não os parâmetros da API; o hook só liga este codec ao router.
# Rastreio: emp-filtros-candidaturas-recrutamento CA-06 e emp-filtro-tipo-usuario-admin CA-03.
@emp-filtros-candidaturas-recrutamento @CA-06
Funcionalidade: Filtros das listagens persistidos na URL
  Como recrutador ou administrador
  Quero que o filtro escolhido fique na URL da listagem
  Para recarregar a página ou voltar do detalhe sem perder status, busca, ordenação e tipo

  # Cada campo lido da URL é validado trocando só ele nos defaults: um default que o próprio schema
  # recusasse faria todo valor da URL cair no default, e a persistência sumiria sem erro nenhum.
  Esquema do Cenário: CA-06 os valores padrão de cada tela são aceitos pelo schema do formulário
    Dado que o filtro da tela "<tela>" está nos valores padrão
    Então o filtro deve ser aceito pelo schema da tela

    Exemplos:
      | tela                         |
      | candidaturas do recrutamento |
      | candidatos da vaga           |
      | vagas do recrutamento        |
      | usuários do admin            |

  Esquema do Cenário: CA-06 URL sem parâmetros abre a tela com o filtro padrão
    Dado que a URL da tela "<tela>" é ""
    Quando eu leio o filtro da URL
    Então o filtro deve estar nos valores padrão da tela

    Exemplos:
      | tela                         |
      | candidaturas do recrutamento |
      | candidatos da vaga           |
      | vagas do recrutamento        |
      | usuários do admin            |

  Cenário: CA-06 recarregar /recrutamento/candidaturas preserva status, busca e ordenação
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "status" do filtro vale "Approved"
    E o campo "search" do filtro vale "Ana Souza"
    E o campo "orderBy" do filtro vale "createdAt_ASC"
    Quando eu gravo o filtro na URL
    E eu recarrego a tela com a URL gravada
    Então a URL gravada deve ser "status=Approved&search=Ana Souza&orderBy=createdAt_ASC"
    E o campo "status" do filtro deve ser "Approved"
    E o campo "search" do filtro deve ser "Ana Souza"
    E o campo "orderBy" do filtro deve ser "createdAt_ASC"

  Cenário: CA-06 recarregar /recrutamento/vagas/[id]/candidatos preserva status, busca e ordenação
    Dado que o filtro da tela "candidatos da vaga" está nos valores padrão
    E o campo "status" do filtro vale "Pending"
    E o campo "search" do filtro vale "maria@email.com"
    E o campo "orderBy" do filtro vale "createdAt_ASC"
    Quando eu gravo o filtro na URL
    E eu recarrego a tela com a URL gravada
    Então o campo "status" do filtro deve ser "Pending"
    E o campo "search" do filtro deve ser "maria@email.com"
    E o campo "orderBy" do filtro deve ser "createdAt_ASC"

  Cenário: CA-06 recarregar /recrutamento/vagas preserva situação, busca e ordenação
    Dado que o filtro da tela "vagas do recrutamento" está nos valores padrão
    E o campo "status" do filtro vale "closed"
    E o campo "search" do filtro vale "Operador"
    E o campo "orderBy" do filtro vale "createdAt_ASC"
    Quando eu gravo o filtro na URL
    E eu recarrego a tela com a URL gravada
    Então a URL gravada deve ser "search=Operador&status=closed&orderBy=createdAt_ASC"
    E o campo "status" do filtro deve ser "closed"
    E o campo "search" do filtro deve ser "Operador"
    E o campo "orderBy" do filtro deve ser "createdAt_ASC"

  @emp-filtro-tipo-usuario-admin @CA-03
  Cenário: CA-03 recarregar /admin/usuarios preserva o tipo de usuário junto da situação
    Dado que o filtro da tela "usuários do admin" está nos valores padrão
    E o campo "situation" do filtro vale "deleted"
    E o campo "userType" do filtro vale "Recruiter"
    Quando eu gravo o filtro na URL
    E eu recarrego a tela com a URL gravada
    Então a URL gravada deve ser "situation=deleted&userType=Recruiter"
    E o campo "userType" do filtro deve ser "Recruiter"
    E o campo "situation" do filtro deve ser "deleted"
    E o campo "search" do filtro deve ser ""

  # URL → formulário → URL: o link compartilhado ou o histórico do navegador reabre a mesma tela,
  # com a query em forma canônica (ordem dos defaults, sem o que é padrão, sem chave estranha).
  Esquema do Cenário: CA-06 ida e volta pela URL normaliza a query sem perder o filtro
    Dado que a URL da tela "<tela>" é "<url>"
    Quando eu leio o filtro da URL
    E eu gravo o filtro na URL
    Então a URL gravada deve ser "<esperado>"

    Exemplos:
      | tela                         | url                                          | esperado                                     |
      | candidaturas do recrutamento | ?status=Rejected&search=joao&orderBy=createdAt_ASC | status=Rejected&search=joao&orderBy=createdAt_ASC |
      | candidaturas do recrutamento | ?orderBy=createdAt_ASC&status=Approved       | status=Approved&orderBy=createdAt_ASC        |
      | candidaturas do recrutamento | ?status=all&search=&orderBy=createdAt_DESC   |                                              |
      | candidaturas do recrutamento | ?page=3&foo=bar                              |                                              |
      | candidatos da vaga           | ?search=maria&status=Processing              | status=Processing&search=maria               |
      | vagas do recrutamento        | ?status=active                               | status=active                                |
      | usuários do admin            | ?userType=Admin&orderBy=createdAt_ASC        | userType=Admin&orderBy=createdAt_ASC         |

  Cenário: CA-06 filtro nos valores padrão deixa a URL da tela limpa
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    Quando eu gravo o filtro na URL
    Então a URL gravada deve ser ""

  Cenário: CA-06 busca só com espaços não vai para a URL
    Dado que o filtro da tela "candidaturas do recrutamento" está nos valores padrão
    E o campo "search" do filtro vale "   "
    Quando eu gravo o filtro na URL
    Então a URL gravada deve ser ""

  Cenário: CA-06 busca vai para a URL sem os espaços das pontas
    Dado que o filtro da tela "candidatos da vaga" está nos valores padrão
    E o campo "search" do filtro vale "  Ana  "
    Quando eu gravo o filtro na URL
    Então a URL gravada deve ser "search=Ana"

  Cenário: CA-06 busca com espaços nas pontas na URL volta aparada ao formulário
    Dado que a URL da tela "candidaturas do recrutamento" é "?search=  Ana  "
    Quando eu leio o filtro da URL
    Então o campo "search" do filtro deve ser "Ana"

  # A URL vem de fora (link antigo, edição manual). Um campo inválido cai no default sozinho: os
  # outros campos da mesma URL continuam valendo.
  Esquema do Cenário: CA-06 valor inválido na URL cai no default sem derrubar os outros campos
    Dado que a URL da tela "<tela>" é "<url>"
    Quando eu leio o filtro da URL
    Então o campo "<invalido>" do filtro deve ser "<padrao>"
    E o campo "<preservado>" do filtro deve ser "<valor>"

    Exemplos:
      | tela                         | url                                        | invalido | padrao         | preservado | valor         |
      | candidaturas do recrutamento | ?status=Aprovada&search=Ana                | status   | all            | search     | Ana           |
      | candidaturas do recrutamento | ?status=NaoSelecionado&orderBy=createdAt_ASC | status | all            | orderBy    | createdAt_ASC |
      | candidaturas do recrutamento | ?orderBy=nome_ASC&status=Rejected          | orderBy  | createdAt_DESC | status     | Rejected      |
      | candidatos da vaga           | ?status=Qualquer&search=joao               | status   | all            | search     | joao          |
      | vagas do recrutamento        | ?status=inactive&search=Operador           | status   | all            | search     | Operador      |

  @emp-filtro-tipo-usuario-admin @CA-03
  Esquema do Cenário: CA-03 tipo de usuário inválido na URL cai em "Todos" sem derrubar os outros campos
    Dado que a URL da tela "usuários do admin" é "<url>"
    Quando eu leio o filtro da URL
    Então o campo "userType" do filtro deve ser "all"
    E o campo "<preservado>" do filtro deve ser "<valor>"

    Exemplos:
      | url                                    | preservado | valor   |
      | ?userType=Recrutador&situation=deleted | situation  | deleted |
      | ?userType=NaoSelecionado&search=ana    | search     | ana     |
      | ?userType=Chefe&orderBy=createdAt_ASC  | orderBy    | createdAt_ASC |

  # O teto da busca é o mesmo do backend (120). Uma URL com texto maior cairia num 400 da API.
  Esquema do Cenário: CA-06 busca na URL respeita o teto de 120 caracteres
    Dado que a URL da tela "candidaturas do recrutamento" traz o campo "search" com <tamanho> caracteres
    Quando eu leio o filtro da URL
    Então o campo "search" do filtro deve ter <esperado> caracteres

    Exemplos:
      | tamanho | esperado |
      | 120     | 120      |
      | 121     | 0        |

  # A mesma regra decide o que vai para a URL e se a tela mostra "Limpar filtros" no estado vazio.
  Esquema do Cenário: CA-06 filtro ativo é tudo o que difere do padrão, inclusive a ordenação
    Dado que o filtro da tela "<tela>" está nos valores padrão
    E o campo "<campo>" do filtro vale "<valor>"
    Então o filtro <resultado> contar como ativo

    Exemplos:
      | tela                         | campo    | valor          | resultado |
      | candidaturas do recrutamento | status   | all            | não deve  |
      | candidaturas do recrutamento | search   |                | não deve  |
      | candidaturas do recrutamento | status   | Approved       | deve      |
      | candidaturas do recrutamento | orderBy  | createdAt_ASC  | deve      |
      | vagas do recrutamento        | search   | Operador       | deve      |
      | usuários do admin            | userType | Admin          | deve      |
