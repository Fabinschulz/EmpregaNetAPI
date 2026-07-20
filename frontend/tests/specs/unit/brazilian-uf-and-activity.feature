#language: pt

Funcionalidade: Normalização de UF e tipo de atividade da empresa
  Como sistema
  Quero normalizar o estado (UF) e o tipo de atividade devolvidos pela API
  Para pré-preencher o formulário de edição corretamente, seja qual for o
  formato em que o backend devolveu o valor (nome do enum, índice numérico
  ou descrição em português)

  Esquema do Cenário: normalizar o estado (UF) vindo como texto
    Quando eu normalizo o estado (texto) "<entrada>"
    Então o estado normalizado deve ser "<esperado>"

    Exemplos:
      | entrada | esperado |
      | SP      | SP       |
      | 25      | SP       |
      | rj      | RJ       |
      | xx      | (vazio)  |
      |         | (vazio)  |

  Esquema do Cenário: normalizar o estado (UF) vindo como índice numérico do enum
    # A API às vezes serializa o enum UF como inteiro em vez do nome (JsonStringEnumConverter
    # não se aplica quando o Newtonsoft é o formatter ativo). O índice 0 é "NaoSelecionado".
    Quando eu normalizo o estado (número) <indice>
    Então o estado normalizado deve ser "<esperado>"

    Exemplos:
      | indice | esperado       |
      | 25     | SP             |
      | 19     | RJ             |
      | 0      | NaoSelecionado |

  Esquema do Cenário: normalizar o tipo de atividade vindo da API (nome do enum ou descrição pt-BR)
    Quando eu normalizo o tipo de atividade "<entrada>"
    Então o tipo de atividade normalizado deve ser "<esperado>"

    Exemplos:
      | entrada     | esperado |
      | Industry    | Industry |
      | Indústria   | Industry |
      | services    | services |
      | Serviços    | services |
      | business    | business |
      | Comércio    | business |
      | inexistente | (vazio)  |
      |             | (vazio)  |
