#language: pt

Funcionalidade: Filtros do feed de vagas na URL
  Como candidato
  Quero que os filtros do feed vivam na URL
  Para poder recarregar a página, voltar do detalhe e compartilhar a busca sem perder nada

  Cenário: URL sem parâmetros produz os filtros padrão
    Dado que a URL do feed é ""
    Quando eu interpreto os filtros do feed
    Então o filtro "sort" deve ser "Recent"
    E nenhum filtro deve estar ativo

  Cenário: preservar a busca textual
    Dado que a URL do feed é "?q=empilhadeira"
    Quando eu interpreto os filtros do feed
    Então o filtro "search" deve ser "empilhadeira"
    E devem estar ativos 1 filtros

  Cenário: aceitar vários valores repetindo a chave
    Dado que a URL do feed é "?shift=SegundoTurno&shift=TerceiroTurno"
    Quando eu interpreto os filtros do feed
    Então a lista "workShifts" deve conter 2 itens
    E a lista "workShifts" deve conter "SegundoTurno"

  Cenário: aceitar vários valores separados por vírgula
    Dado que a URL do feed é "?req=Empilhadeira,WMS"
    Quando eu interpreto os filtros do feed
    Então a lista "requirements" deve conter 2 itens

  Cenário: remover valores repetidos
    Dado que a URL do feed é "?shift=SegundoTurno&shift=SegundoTurno"
    Quando eu interpreto os filtros do feed
    Então a lista "workShifts" deve conter 1 itens

  # A URL vem de fora: link antigo, valor removido do vocabulário, alguém editando à mão.
  # Descartar o que não se reconhece é melhor do que devolver um feed vazio com erro.
  Esquema do Cenário: descartar em silêncio valores que não existem
    Dado que a URL do feed é "<url>"
    Quando eu interpreto os filtros do feed
    Então a lista "<lista>" deve conter 0 itens

    Exemplos:
      | url                 | lista            |
      | ?model=Presencial   | workModels       |
      | ?shift=Noturno      | workShifts       |
      | ?exp=Pleno          | experienceLevels |
      | ?area=Development   | areas            |
      | ?uf=XX              | states           |
      | ?company=abc        | companyIds       |
      | ?company=-3         | companyIds       |

  # Requisitos e benefícios são listas abertas servidas pela API: o cliente não as valida.
  # Um valor inexistente simplesmente não casa com vaga nenhuma — resultado correto.
  Cenário: requisito desconhecido é preservado, não descartado
    Dado que a URL do feed é "?req=Requisito Que Nao Existe"
    Quando eu interpreto os filtros do feed
    Então a lista "requirements" deve conter 1 itens

  Cenário: ordenação inválida volta ao padrão
    Dado que a URL do feed é "?sort=Aleatorio"
    Quando eu interpreto os filtros do feed
    Então o filtro "sort" deve ser "Recent"

  Cenário: normalizar a UF para maiúsculas
    Dado que a URL do feed é "?uf=mg"
    Quando eu interpreto os filtros do feed
    Então a lista "states" deve conter "MG"

  Cenário: aceitar o turno em qualquer caixa
    Dado que a URL do feed é "?shift=segundoturno"
    Quando eu interpreto os filtros do feed
    Então a lista "workShifts" deve conter "SegundoTurno"

  Cenário: filtro de vagas PcD é lido da URL
    Dado que a URL do feed é "?pcd=1"
    Quando eu interpreto os filtros do feed
    Então o filtro "onlyPcd" deve ser "true"
    E devem estar ativos 1 filtros

  Esquema do Cenário: ida e volta pela URL preserva os filtros
    Dado que a URL do feed é "<url>"
    Quando eu interpreto os filtros do feed
    E eu serializo os filtros de volta para a URL
    Então a URL serializada deve ser "<esperado>"

    Exemplos:
      | url                                 | esperado                            |
      | ?q=empilhadeira                     | q=empilhadeira                      |
      | ?shift=SegundoTurno&exp=AteUmAno    | shift=SegundoTurno&exp=AteUmAno     |
      | ?salary=2300-3000&since=Last7Days   | salary=2300-3000&since=Last7Days    |
      | ?uf=MG&uf=SP                        | uf=MG&uf=SP                         |
      | ?pcd=1                              | pcd=1                               |

  # Uma URL sem filtro nenhum fica `/vagas` limpa, e não `/vagas?q=&sort=Recent`.
  Cenário: valores no padrão não vão para a URL
    Dado que a URL do feed é "?sort=Recent&q=&pcd=0"
    Quando eu interpreto os filtros do feed
    E eu serializo os filtros de volta para a URL
    Então a URL serializada deve ser ""

  Cenário: contar filtros ativos somando cada valor selecionado
    Dado que a URL do feed é "?q=ajudante&shift=SegundoTurno&shift=TerceiroTurno&salary=2300-3000"
    Quando eu interpreto os filtros do feed
    Então devem estar ativos 4 filtros

  # As faixas foram calibradas para o mercado atendido: degraus estreitos na base, onde está a
  # maior parte das vagas do polo industrial.
  Esquema do Cenário: converter a faixa salarial em limites para a API
    Dado que a URL do feed é "?salary=<faixa>"
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "salaryMin" da API deve ser "<min>"
    E o parâmetro "salaryMax" da API deve ser "<max>"

    Exemplos:
      | faixa       | min  | max  |
      | ate-1800    |      | 1800 |
      | 2300-3000   | 2300 | 3000 |
      | acima-7000  | 7000 |      |

  # Relevância sem termo de busca não tem o que ranquear; o backend também degrada, mas evitar
  # a viagem mantém a chave de cache honesta.
  Cenário: relevância sem busca cai para mais recentes ao chamar a API
    Dado que a URL do feed é "?sort=Relevance"
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "sort" da API deve ser "Recent"

  Cenário: relevância com busca é preservada
    Dado que a URL do feed é "?sort=Relevance&q=empilhadeira"
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "sort" da API deve ser "Relevance"

  Cenário: turno vira o parâmetro shift na API
    Dado que a URL do feed é "?shift=TerceiroTurno"
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "shift" da API deve ser "TerceiroTurno"

  Cenário: filtros vazios não viram parâmetros da API
    Dado que a URL do feed é ""
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "city" da API deve estar ausente
    E o parâmetro "search" da API deve estar ausente
    E o parâmetro "pcd" da API deve estar ausente

  # Contrato que as pills de filtro accionam. O componente não é coberto aqui (a suite não
  # renderiza React), mas alternar → URL → contagem de ativos é o que ele depende.
  Cenário: escolher um valor na pill entra na URL e conta como filtro ativo
    Dado que a URL do feed é ""
    Quando eu interpreto os filtros do feed
    E eu alterno o valor "SegundoTurno" no filtro "workShifts"
    E eu serializo os filtros de volta para a URL
    Então a URL serializada deve ser "shift=SegundoTurno"
    E devem estar ativos 1 filtros

  Cenário: clicar de novo no mesmo valor da pill remove o filtro
    Dado que a URL do feed é "?shift=SegundoTurno"
    Quando eu interpreto os filtros do feed
    E eu alterno o valor "SegundoTurno" no filtro "workShifts"
    E eu serializo os filtros de volta para a URL
    Então a URL serializada deve ser ""
    E nenhum filtro deve estar ativo

  Cenário: pills diferentes acumulam em vez de se substituírem
    Dado que a URL do feed é ""
    Quando eu interpreto os filtros do feed
    E eu alterno o valor "SegundoTurno" no filtro "workShifts"
    E eu alterno o valor "Producao" no filtro "areas"
    E eu alterno o valor "Fretado" no filtro "benefits"
    Então devem estar ativos 3 filtros

  # ---------------------------------------------------------------------------------------------
  # emp-filtros-vagas-localizacao — Estado e Cidade
  # ---------------------------------------------------------------------------------------------

  @emp-filtros-vagas-localizacao @CA-07
  Esquema do Cenário: CA-07 Estado e Cidade repetidos sobrevivem à ida e volta pela URL
    Dado que a URL do feed é "<url>"
    Quando eu interpreto os filtros do feed
    E eu serializo os filtros de volta para a URL
    Então a URL serializada deve ser "<esperado>"

    Exemplos:
      | url                                         | esperado                                    |
      | ?uf=CE&city=Fortaleza                       | city=Fortaleza&uf=CE                        |
      | ?uf=CE&uf=SP&city=Fortaleza&city=Campinas   | city=Fortaleza&city=Campinas&uf=CE&uf=SP    |

  # Recarregar = interpretar de novo a URL que o próprio feed escreveu. Cidade com espaço no nome é
  # o caso comum no interior ("Juazeiro do Norte") e passa pela codificação da query.
  @emp-filtros-vagas-localizacao @CA-07
  Cenário: CA-07 recarregar a página mantém Estados e Cidades escolhidos
    Dado que a URL do feed é "?uf=CE&uf=SP&city=Juazeiro do Norte&city=Campinas"
    Quando eu interpreto os filtros do feed
    E eu serializo os filtros de volta para a URL
    E eu interpreto de novo os filtros a partir da URL serializada
    Então a lista "states" deve conter 2 itens
    E a lista "states" deve conter "CE"
    E a lista "states" deve conter "SP"
    E a lista "cities" deve conter 2 itens
    E a lista "cities" deve conter "Juazeiro do Norte"
    E a lista "cities" deve conter "Campinas"
    E devem estar ativos 4 filtros

  # A URL usa `uf`, a API usa `state`: é nesta tradução que um filtro se perderia em silêncio.
  @emp-filtros-vagas-localizacao @CA-07
  Cenário: CA-07 Estado e Cidade da URL viram os parâmetros state e city da API
    Dado que a URL do feed é "?uf=CE&uf=SP&city=Fortaleza"
    Quando eu interpreto os filtros do feed
    E eu converto os filtros em parâmetros da API
    Então o parâmetro "state" da API deve ser "CE,SP"
    E o parâmetro "city" da API deve ser "Fortaleza"

  @emp-filtros-vagas-localizacao @CA-04
  Cenário: CA-04 com um Estado selecionado só aparecem as cidades dele
    Dado que o vocabulário do feed traz as cidades:
      | uf | cidades                        |
      | CE | Fortaleza, Juazeiro do Norte   |
      | MG | Belo Horizonte                 |
      | SP | Campinas, São Paulo            |
    Quando eu calculo os grupos de cidade para os Estados "CE"
    Então devem aparecer 1 grupos de cidade
    E o grupo de cidade "Ceará" deve listar "Fortaleza, Juazeiro do Norte"

  @emp-filtros-vagas-localizacao @CA-04
  Cenário: CA-04 com dois Estados selecionados aparecem as cidades dos dois
    Dado que o vocabulário do feed traz as cidades:
      | uf | cidades                        |
      | CE | Fortaleza, Juazeiro do Norte   |
      | MG | Belo Horizonte                 |
      | SP | Campinas, São Paulo            |
    Quando eu calculo os grupos de cidade para os Estados "SP, CE"
    Então devem aparecer 2 grupos de cidade
    E o grupo de cidade "Ceará" deve listar "Fortaleza, Juazeiro do Norte"
    E o grupo de cidade "São Paulo" deve listar "Campinas, São Paulo"
    E não deve aparecer o grupo de cidade "Minas Gerais"

  @emp-filtros-vagas-localizacao @CA-04
  Cenário: CA-04 Estado sem vagas não mostra cidade nenhuma
    Dado que o vocabulário do feed traz as cidades:
      | uf | cidades                        |
      | CE | Fortaleza, Juazeiro do Norte   |
      | SP | Campinas, São Paulo            |
    Quando eu calculo os grupos de cidade para os Estados "BA"
    Então devem aparecer 0 grupos de cidade

  @emp-filtros-vagas-localizacao @CA-04
  Cenário: CA-04 Estado sem vagas junto de outro com vagas mostra só as cidades do que tem
    Dado que o vocabulário do feed traz as cidades:
      | uf | cidades                        |
      | CE | Fortaleza, Juazeiro do Norte   |
      | SP | Campinas, São Paulo            |
    Quando eu calculo os grupos de cidade para os Estados "BA, CE"
    Então devem aparecer 1 grupos de cidade
    E o grupo de cidade "Ceará" deve listar "Fortaleza, Juazeiro do Norte"

  @emp-filtros-vagas-localizacao @CA-05
  Cenário: CA-05 sem Estado selecionado aparecem todas as cidades, agrupadas pelo nome do Estado
    Dado que o vocabulário do feed traz as cidades:
      | uf | cidades                        |
      | CE | Fortaleza, Juazeiro do Norte   |
      | MG | Belo Horizonte                 |
      | SP | Campinas, São Paulo            |
    Quando eu calculo os grupos de cidade para os Estados ""
    Então devem aparecer 3 grupos de cidade
    E o grupo de cidade "Ceará" deve listar "Fortaleza, Juazeiro do Norte"
    E o grupo de cidade "Minas Gerais" deve listar "Belo Horizonte"
    E o grupo de cidade "São Paulo" deve listar "Campinas, São Paulo"

  @emp-filtros-vagas-localizacao @CA-05
  Cenário: CA-05 vocabulário sem cidades não produz grupo nenhum
    Dado que o vocabulário do feed não traz cidades
    Quando eu calculo os grupos de cidade para os Estados ""
    Então devem aparecer 0 grupos de cidade

  # Contrato: o grupo de cidades é rotulado por `ufFullLabel`. Um código sem rótulo apareceria na
  # gaveta como título vazio. Cobre as 27 UFs do seletor de Estado de uma vez.
  @emp-filtros-vagas-localizacao @contrato
  Cenário: Contrato - toda UF do seletor de Estado tem rótulo por extenso nos grupos de cidade
    Dado que o vocabulário do feed traz uma cidade em cada UF do seletor de Estado
    Quando eu calculo os grupos de cidade para os Estados ""
    Então devem aparecer 27 grupos de cidade
    E cada grupo de cidade deve ter o nome por extenso da sua UF
