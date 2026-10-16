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
