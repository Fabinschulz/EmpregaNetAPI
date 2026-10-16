#language: pt

Funcionalidade: Aviso de vaga encerrada
  Como visitante que abre o link de uma vaga já encerrada
  Quero entender que o processo seletivo terminou
  Para não achar que a página está quebrada

  Esquema do Cenário: motivo conforme o status
    Quando eu resumo o encerramento da vaga com status "<status>"
    Então o motivo deve ser "<motivo>"

    Exemplos:
      | status              | motivo                                       |
      | ClosedByFulfillment | Todas as posições foram preenchidas.         |
      | ClosedManually      | A empresa encerrou as inscrições para esta vaga. |

  Esquema do Cenário: pílula e título conforme o status e o total de posições
    Quando eu resumo o encerramento da vaga com status "<status>" e <posicoes> posições
    Então a pílula deve ser "<pilula>"
    E o título deve ser "<titulo>"

    Exemplos:
      | status              | posicoes | pilula           | titulo                                  |
      | ClosedByFulfillment | 1        | Vaga preenchida  | Boas notícias: esta vaga foi preenchida |
      | ClosedByFulfillment | 3        | Vagas preenchidas | Boas notícias: esta vaga foi preenchida |
      | ClosedManually      | 1        | Encerrada        | Processo seletivo encerrado             |

  Cenário: sem total de posições o rótulo fica no plural
    Quando eu resumo o encerramento da vaga com status "ClosedByFulfillment"
    Então a pílula deve ser "Vagas preenchidas"

  Esquema do Cenário: rótulo de status da vaga conforme o total de posições
    Então o rótulo de status "<status>" com <posicoes> posições deve ser "<rotulo>"

    Exemplos:
      | status              | posicoes | rotulo            |
      | ClosedByFulfillment | 1        | Vaga preenchida   |
      | ClosedByFulfillment | 2        | Vagas preenchidas |
      | Active              | 1        | Ativa             |

  Cenário: faixa salarial com "a" apenas em vaga encerrada
    Quando eu formato a faixa de 1800 a 2400 com o conector " a "
    Então a faixa deve ser "R$ 1.800 a R$ 2.400"

  Cenário: faixa salarial padrão mantém o hífen
    Quando eu formato a faixa de 1800 a 2400 com o conector padrão
    Então a faixa deve ser "R$ 1.800 - R$ 2.400"

  Cenário: data de encerramento por extenso
    Quando eu resumo o encerramento da vaga publicada em "2026-10-01T12:00:00" e encerrada em "2026-10-16T12:00:00"
    Então a frase deve ser "Esta vaga foi encerrada em 16 de outubro de 2026."
    E o período aberto deve ser "Ficou aberta por 15 dias"

  Cenário: data da API no formato dd/MM/yyyy
    Quando eu resumo o encerramento da vaga publicada em "01/10/2026" e encerrada em "02/10/2026"
    Então a frase deve ser "Esta vaga foi encerrada em 2 de outubro de 2026."
    E o período aberto deve ser "Ficou aberta por 1 dia"

  Cenário: sem data de encerramento não se inventa data
    Quando eu resumo o encerramento da vaga publicada em "2026-10-01T12:00:00" e sem data de encerramento
    Então a frase deve ser "Esta vaga foi encerrada."
    E não deve haver período aberto
