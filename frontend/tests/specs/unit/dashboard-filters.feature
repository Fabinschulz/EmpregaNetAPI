#language: pt

Funcionalidade: Recorte do dashboard de métricas
  Como responsável pelo recrutamento
  Quero que o recorte escolhido no cabeçalho chegue ao servidor exatamente como escolhi
  Para que os números da tela sejam auditáveis

  Contexto:
    Dado que o formulário de filtros do dashboard está com os valores padrão

  Cenário: o período padrão é últimos 30 dias e nada mais é enviado
    Quando eu converto o formulário em recorte
    Então o recorte deve existir
    E o parâmetro "period" deve ser "Last30Days"
    E o parâmetro "from" não deve ser enviado
    E o parâmetro "to" não deve ser enviado
    E o parâmetro "companyId" não deve ser enviado
    E o parâmetro "state" não deve ser enviado
    E o parâmetro "area" não deve ser enviado
    E o parâmetro "status" não deve ser enviado

  # Enviar `?state=` faria o servidor receber uma UF que não reconhece, e o painel voltaria
  # vazio sem nada na tela a explicar por quê.
  Cenário: seleção vazia não vira parâmetro
    Dado que o campo "states" do formulário é a lista ""
    E que o campo "areas" do formulário é a lista ""
    E que o campo "applicationStatus" do formulário é "all"
    E que o campo "companyId" do formulário é "all"
    Quando eu converto o formulário em recorte
    Então o parâmetro "state" não deve ser enviado
    E o parâmetro "area" não deve ser enviado
    E o parâmetro "status" não deve ser enviado
    E o parâmetro "companyId" não deve ser enviado

  Cenário: as seleções múltiplas chegam como lista
    Dado que o campo "states" do formulário é a lista "SP,MG"
    E que o campo "areas" do formulário é a lista "Producao"
    Quando eu converto o formulário em recorte
    Então o parâmetro de lista "state" deve conter 2 itens
    E o parâmetro de lista "state" deve conter "SP"
    E o parâmetro de lista "area" deve conter 1 itens

  Cenário: a empresa escolhida chega como número
    Dado que o campo "companyId" do formulário é "42"
    Quando eu converto o formulário em recorte
    Então o parâmetro "companyId" deve ser o número 42

  Cenário: o status escolhido é enviado
    Dado que o campo "applicationStatus" do formulário é "Approved"
    Quando eu converto o formulário em recorte
    Então o parâmetro "status" deve ser "Approved"

  # As datas só existem no período personalizado: mandá-las junto de "últimos 7 dias" faria o
  # servidor ter de decidir qual das duas instruções obedecer.
  Cenário: datas são ignoradas fora do período personalizado
    Dado que o campo "from" do formulário é "2026-07-01"
    E que o campo "to" do formulário é "2026-07-31"
    Quando eu converto o formulário em recorte
    Então o parâmetro "from" não deve ser enviado
    E o parâmetro "to" não deve ser enviado

  Cenário: período personalizado completo envia as duas datas
    Dado que o campo "period" do formulário é "Custom"
    E que o campo "from" do formulário é "2026-07-01"
    E que o campo "to" do formulário é "2026-07-31"
    Quando eu converto o formulário em recorte
    Então o recorte deve existir
    E o parâmetro "from" deve ser "2026-07-01"
    E o parâmetro "to" deve ser "2026-07-31"

  # Enquanto o utilizador digita a primeira data, o formulário passa por estados inválidos.
  # Emitir recorte em cada um deles produziria uma sequência de erros de validação do servidor.
  Esquema do Cenário: período personalizado incompleto não produz recorte
    Dado que o campo "period" do formulário é "Custom"
    E que o campo "from" do formulário é "<de>"
    E que o campo "to" do formulário é "<ate>"
    Quando eu converto o formulário em recorte
    Então o recorte não deve existir

    Exemplos:
      | de         | ate        |
      |            |            |
      | 2026-07-01 |            |
      |            | 2026-07-31 |
      | 2026-08-30 | 2026-08-01 |

  Cenário: a chave de cache ignora a ordem das seleções
    Dado que o campo "states" do formulário é a lista "SP,MG"
    Quando eu converto o formulário em recorte
    E eu guardo a chave de cache do recorte
    E que o campo "states" do formulário é a lista "MG,SP"
    E eu converto o formulário em recorte
    Então a chave de cache do recorte deve ser igual à guardada

  Cenário: a chave de cache muda quando o período muda
    Quando eu converto o formulário em recorte
    E eu guardo a chave de cache do recorte
    E que o campo "period" do formulário é "Last7Days"
    E eu converto o formulário em recorte
    Então a chave de cache do recorte deve ser diferente da guardada
