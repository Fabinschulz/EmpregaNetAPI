#language: pt

# Máquina de estados pura de `useUrlSyncedParams` (`shared/hooks/url-sync-state.ts`). O App Router não
# remonta a página quando só a query muda, então o hook precisa separar o eco das próprias escritas
# (`router.replace`) de uma mudança feita por fora (link do menu para a mesma rota, voltar do detalhe).
# Errar para um lado remonta o formulário à toa; errar para o outro ignora a navegação do usuário.
# Rastreio: emp-filtros-candidaturas-recrutamento CA-06/CA-07/CA-08 e emp-filtro-tipo-usuario-admin CA-03.
@emp-filtros-candidaturas-recrutamento
Funcionalidade: Conciliação entre o filtro da listagem e a URL do router
  Como recrutador ou administrador
  Quero que o filtro da tela acompanhe a URL sem remontar o formulário enquanto eu digito
  Para que recarregar, voltar do detalhe e clicar no menu levem sempre ao filtro que a URL mostra

  @CA-06 @emp-filtro-tipo-usuario-admin @CA-03
  Cenário: CA-06 query igual à do estado não muda nada e devolve o mesmo objeto de estado
    Dado que a tela abriu com a URL "status=Approved"
    Quando o router devolve a query "status=Approved"
    Então a query não deve ser tratada como mudança externa
    E o estado deve ser o mesmo objeto de antes
    E os valores não devem ter sido relidos da URL
    E o formulário deve ter remontado 0 vezes

  @CA-06 @CA-07 @emp-filtro-tipo-usuario-admin @CA-03
  Cenário: CA-06 o eco da própria escrita não é mudança externa e sai dos pendentes
    Dado que a tela abriu com a URL ""
    Quando o formulário escreve a query "status=Approved"
    Então o router.replace deve ser chamado
    E as escritas pendentes devem ser "status=Approved"
    Quando o router devolve a query "status=Approved"
    Então a query não deve ser tratada como mudança externa
    E não deve haver escrita pendente
    E os valores não devem ter sido relidos da URL
    E o formulário deve ter remontado 0 vezes
    E os valores do filtro devem ser os escritos para "status=Approved"

  # O eco mais novo invalida as escritas anteriores a ele, que o router pode ter descartado.
  @CA-06 @CA-07
  Cenário: CA-06 o eco de uma escrita descarta junto as escritas anteriores a ela
    Dado que a tela abriu com a URL ""
    Quando o formulário escreve a query "status=Approved"
    E o formulário escreve a query "status=Approved&search=Ana"
    E o router devolve a query "status=Approved&search=Ana"
    Então a query não deve ser tratada como mudança externa
    E não deve haver escrita pendente
    E o formulário deve ter remontado 0 vezes

  @CA-06 @CA-07
  Cenário: CA-06 ecos chegando em ordem não remontam o formulário
    Dado que a tela abriu com a URL ""
    Quando o formulário escreve a query "status=Approved"
    E o formulário escreve a query "status=Rejected"
    E o router devolve a query "status=Approved"
    Então a query não deve ser tratada como mudança externa
    E as escritas pendentes devem ser "status=Rejected"
    Quando o router devolve a query "status=Rejected"
    Então a query não deve ser tratada como mudança externa
    E não deve haver escrita pendente
    E os valores não devem ter sido relidos da URL
    E o formulário deve ter remontado 0 vezes
    E os valores do filtro devem ser os escritos para "status=Rejected"

  # Clique no menu lateral para a mesma rota, sem query: o App Router não remonta a página, então
  # só a conciliação percebe que o filtro tem de voltar ao que a URL diz.
  @CA-07 @emp-filtro-tipo-usuario-admin @CA-03
  Esquema do Cenário: CA-07 query que o hook não escreveu é mudança externa e remonta o formulário
    Dado que a tela abriu com a URL "<inicial>"
    Quando o router devolve a query "<externa>"
    Então a query deve ser tratada como mudança externa
    E os valores devem ter sido relidos da URL "<externa>"
    E o formulário deve ter remontado 1 vezes
    E não deve haver escrita pendente
    E a query conhecida deve ser "<externa>"

    Exemplos:
      | inicial                              | externa                        |
      | status=Approved&search=Ana           |                                |
      | situation=deleted&userType=Recruiter |                                |
      |                                      | status=Pending                 |
      | status=Approved                      | status=Rejected&orderBy=createdAt_ASC |

  @CA-07
  Cenário: CA-07 navegação externa com escrita ainda pendente descarta os pendentes
    Dado que a tela abriu com a URL ""
    Quando o formulário escreve a query "status=Approved"
    E o router devolve a query "status=Pending"
    Então a query deve ser tratada como mudança externa
    E os valores devem ter sido relidos da URL "status=Pending"
    E o formulário deve ter remontado 1 vezes
    E não deve haver escrita pendente

  # Regressão obrigatória: esta sequência já quebrou. Filtrar, limpar antes do eco (o router descarta
  # a primeira navegação e o eco "" é igual à query do estado, então nunca aparece como mudança),
  # filtrar de novo e receber o eco. Uma escrita velha que sobrasse nos pendentes faria a navegação
  # externa seguinte para "" passar por eco, e o formulário ficaria com o filtro antigo.
  @CA-07 @CA-08 @regressao
  Cenário: CA-07 regressão - filtrar, limpar antes do eco e filtrar de novo não mascara a navegação externa
    Dado que a tela abriu com a URL ""
    Quando o formulário escreve a query "status=Approved"
    E o formulário escreve a query ""
    # A volta para a query atual é comparada com a última escrita pendente, não com a query do estado.
    Então o router.replace deve ser chamado
    E as escritas pendentes devem ser "status=Approved | "
    Quando o router devolve a query ""
    Então a query não deve ser tratada como mudança externa
    E o estado deve ser o mesmo objeto de antes
    Quando o formulário escreve a query "status=Approved"
    Então o router.replace deve ser chamado
    Quando o router devolve a query "status=Approved"
    Então a query não deve ser tratada como mudança externa
    E não deve haver escrita pendente
    E o formulário deve ter remontado 0 vezes
    Quando o router devolve a query ""
    Então a query deve ser tratada como mudança externa
    E os valores devem ter sido relidos da URL ""
    E o formulário deve ter remontado 1 vezes
    E não deve haver escrita pendente

  # Ex.: um espaço a mais na busca, que a serialização remove — a query não muda.
  @CA-06
  Esquema do Cenário: CA-06 escrever a mesma query da última conhecida não chama o router, mas atualiza os valores
    Dado que a tela abriu com a URL "<inicial>"
    E o formulário escreveu antes as queries "<antes>"
    Quando o formulário escreve a query "<repetida>"
    Então o router.replace não deve ser chamado
    E os valores do filtro devem ser os escritos para "<repetida>"
    E a query conhecida deve ser "<repetida>"
    E o formulário deve ter remontado 0 vezes

    Exemplos:
      | inicial         | antes           | repetida        |
      | status=Approved |                 | status=Approved |
      |                 | status=Rejected | status=Rejected |

  @CA-08 @emp-filtro-tipo-usuario-admin @CA-03
  Esquema do Cenário: CA-08 "Limpar filtros" (reset) remonta o formulário
    Dado que a tela abriu com a URL "<inicial>"
    Quando o reset escreve a query ""
    Então o formulário deve ter remontado 1 vezes
    E os valores do filtro devem ser os escritos para ""
    E o router.replace <chamada> ser chamado

    Exemplos:
      | inicial                              | chamada  |
      | status=Approved&search=Ana           | deve     |
      | situation=deleted&userType=Recruiter | deve     |
      |                                      | não deve |

  @CA-08
  Cenário: CA-08 o eco do reset não remonta o formulário uma segunda vez
    Dado que a tela abriu com a URL "status=Approved"
    Quando o reset escreve a query ""
    E o router devolve a query ""
    Então a query não deve ser tratada como mudança externa
    E o formulário deve ter remontado 1 vezes
    E os valores não devem ter sido relidos da URL
