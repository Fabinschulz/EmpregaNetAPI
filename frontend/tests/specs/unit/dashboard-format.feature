#language: pt

Funcionalidade: Apresentação dos números do dashboard
  Como responsável pelo recrutamento
  Quero ler os números no formato brasileiro e com o sinal explícito
  Para não confundir crescimento com queda nem 1.284 com 1,284

  Esquema do Cenário: contagem no formato brasileiro
    Quando eu formato a contagem <valor>
    Então o número formatado deve ser "<esperado>"

    Exemplos:
      | valor | esperado |
      | 0     | 0        |
      | 42    | 42       |
      | 1284  | 1.284    |

  # O eixo do gráfico não tem largura para "1.284"; o valor exato continua no tooltip.
  Esquema do Cenário: contagem abreviada para o eixo do gráfico
    Quando eu formato a contagem compacta <valor>
    Então o número formatado deve ser "<esperado>"

    Exemplos:
      | valor | esperado |
      | 999   | 999      |
      | 1200  | 1,2 mil  |

  Esquema do Cenário: percentagem sem casa decimal quando é inteira
    Quando eu formato a percentagem <valor>
    Então o número formatado deve ser "<esperado>"

    Exemplos:
      | valor | esperado |
      | 38    | 38%      |
      | 12.4  | 12,4%    |
      | 0     | 0%       |

  # Sinal negativo tipográfico (−), não hífen: ao lado de dígitos, o hífen fica curto demais
  # para ser lido como sinal.
  Esquema do Cenário: variação sempre com sinal explícito
    Quando eu formato a variação <valor>
    Então o número formatado deve ser "<esperado>"

    Exemplos:
      | valor | esperado |
      | 12.4  | +12,4%   |
      | -8    | −8%      |
      | 0     | 0%       |

  Cenário: taxa é formatada como percentagem, contagem como número
    Quando eu formato o indicador 30 com unidade "percent"
    Então o número formatado deve ser "30%"

  Cenário: indicador sem unidade é contagem
    Quando eu formato o indicador 1284 sem unidade
    Então o número formatado deve ser "1.284"

  Esquema do Cenário: plural correto na contagem de dias
    Quando eu formato <valor> dias
    Então o número formatado deve ser "<esperado>"

    Exemplos:
      | valor | esperado  |
      | 1     | 1 dia     |
      | 30    | 30 dias   |
      | 1200  | 1.200 dias |
