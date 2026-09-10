#language: pt

# A regressão E2E de 2026-09-01 encontrou "Encerrar vaga" a executar com **um clique**, sem
# confirmação, numa tela que não mostrava o estado da vaga — e o botão continuava a aparecer
# depois de encerrada, devolvendo erro ao segundo clique. Como vaga encerrada não volta a ser
# activada (PRD D4), a acção é irreversível: a confirmação e a contagem do efeito são o
# comportamento, não enfeite.

Funcionalidade: Encerrar vaga com confirmação
  Como recrutador
  Quero ver o estado da vaga e saber quantas candidaturas o encerramento afecta
  Para não encerrar por engano um processo que não posso reabrir

  Cenário: a gestão mostra o estado de uma vaga activa
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    Quando eu abro a gestão da vaga
    Então o estado exibido na gestão da vaga deve ser "Ativa"
    E a acção "Encerrar vaga" deve estar disponível na gestão

  # CA-18: nada de oferecer a acção e deixar a API recusar o segundo clique.
  Cenário: vaga encerrada mostra o estado e não oferece encerrar
    Dado que a vaga #36 está encerrada
    Quando eu abro a gestão da vaga
    Então o estado exibido na gestão da vaga deve ser "Encerrada"
    E a acção "Encerrar vaga" não deve estar disponível na gestão

  # CA-17: a confirmação diz o efeito antes de executar.
  Esquema do Cenário: a confirmação informa quantas candidaturas serão afectadas
    Dado que a vaga #36 está activa com <abertas> candidaturas em aberto
    Quando eu escolho encerrar a vaga
    Então a confirmação de encerramento deve estar aberta
    E a confirmação de encerramento deve dizer "<efeito>"
    E a confirmação de encerramento deve avisar que a vaga não pode ser reativada

    Exemplos:
      | abertas | efeito                                       |
      | 0       | Nenhuma candidatura em aberto será cancelada |
      | 1       | 1 candidatura em aberto será cancelada       |
      | 3       | 3 candidaturas em aberto serão canceladas    |

  Cenário: abandonar a confirmação não encerra a vaga
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    Quando eu escolho encerrar a vaga
    E eu abandono a confirmação de encerramento
    Então a API não deve ter recebido o encerramento
    E a confirmação de encerramento deve estar fechada
    E o estado exibido na gestão da vaga deve ser "Ativa"

  # A resposta deixou de ser `bool`: o corpo diz o efeito, e é dele que sai o feedback.
  Cenário: confirmar encerra a vaga e informa o efeito consumado
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    Quando eu escolho encerrar a vaga
    E eu confirmo o encerramento
    Então a API de vagas deve ter recebido "PUT /api/jobs/36/close"
    E o pedido de encerramento não deve ter corpo
    E o feedback de encerramento deve dizer "3 candidaturas em aberto foram canceladas"
    E a confirmação de encerramento deve estar fechada

  Esquema do Cenário: o feedback concorda em número com o efeito devolvido pela API
    Dado que a vaga #36 está activa com <abertas> candidaturas em aberto
    Quando eu escolho encerrar a vaga
    E eu confirmo o encerramento
    Então o feedback de encerramento deve dizer "<efeito>"

    Exemplos:
      | abertas | efeito                                       |
      | 0       | Nenhuma candidatura em aberto foi cancelada  |
      | 1       | 1 candidatura em aberto foi cancelada        |
      | 3       | 3 candidaturas em aberto foram canceladas    |

  # Regressão de contrato: a resposta antiga era a string "Vaga encerrada com sucesso." e o service
  # devolvia-a sem parse, por isso a UI não tinha como dizer o efeito do encerramento. O contrato
  # novo tem de recusar o corpo antigo, e não conviver com ele.
  Cenário: a resposta antiga, em texto, deve ser recusada
    Quando eu leio a resposta de encerramento em texto "Vaga encerrada com sucesso."
    Então a leitura do encerramento deve falhar

  Esquema do Cenário: resposta de encerramento incompleta deve ser recusada
    Quando eu leio uma resposta de encerramento sem "<campo>"
    Então a leitura do encerramento deve falhar no campo "<campo>"

    Exemplos:
      | campo                |
      | jobId                |
      | closedAt             |
      | affectedApplications |

  # `openApplicationsCount` deixou de vir no detalhe da vaga: `GET /api/jobs/{id}` é anónimo e
  # cacheado 5 min, por isso expunha a contagem e podia servi-la velha. Agora é leitura própria,
  # autenticada e sem cache, feita **ao abrir a confirmação** — o instante em que o número decide.
  Cenário: a contagem é lida ao abrir a confirmação, não com o detalhe da vaga
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    Quando eu abro a gestão da vaga
    Então a contagem de candidaturas em aberto não deve ter sido lida
    Quando eu escolho encerrar a vaga
    Então a API deve ter lido "GET /api/jobs/36/open-applications-count"
    E a confirmação de encerramento deve dizer "3 candidaturas em aberto serão canceladas"

  # Sem número não se afirma número: cair para zero diria "nenhuma candidatura será cancelada"
  # sobre uma vaga que pode ter fila — a promessa falsa que esta feature existe para corrigir.
  Cenário: contagem indisponível não vira zero na confirmação
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    E que a leitura da contagem vai falhar
    Quando eu escolho encerrar a vaga
    Então a confirmação de encerramento deve dizer "Não foi possível verificar quantas candidaturas"
    E a confirmação de encerramento não deve dizer "Nenhuma candidatura em aberto"
    E a confirmação de encerramento deve avisar que a vaga não pode ser reativada
    E o encerramento deve continuar possível

  # Enquanto a contagem não chega, a confirmação não arrisca um número.
  Cenário: enquanto a contagem não chega, a confirmação não afirma um número
    Dado que a vaga #36 está activa com 3 candidaturas em aberto
    Quando eu escolho encerrar a vaga sem esperar pela contagem
    Então a confirmação de encerramento deve dizer "Verificando quantas candidaturas em aberto"
    E a confirmação de encerramento não deve dizer "Nenhuma candidatura em aberto"

  # A contagem é contrato próprio: um corpo sem o campo não pode virar zero silencioso.
  Cenário: resposta da contagem sem o campo deve ser recusada
    Quando eu leio uma resposta de contagem sem "openApplicationsCount"
    Então a leitura da contagem deve falhar no campo "openApplicationsCount"
