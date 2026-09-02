#language: pt

# O status é lido pelo **nome do enum**, mas o contrato de leitura da candidatura declara
# `status: z.string()` — logo um valor desconhecido **não** quebra o parse da listagem. Quem o
# reconhece é `applicationStatusSchema` (`z.enum`), consultado pelo badge: sem entrada aqui,
# `parseApplicationStatus` devolve null, o badge cai no fallback neutro e o candidato lê o
# identificador cru do backend ("CanceledByCandidate") em vez do rótulo em português.
# A falha é cosmética, não é erro de tela — e por isso passa despercebida sem estes cenários.

Funcionalidade: Vocabulário de status de candidatura
  Como candidato e como recrutador
  Quero ler o status da candidatura em português e com a autoria certa
  Para saber quem encerrou o processo e o que ainda posso fazer

  Cenário: todo status conhecido pela API tem rótulo em português
    Quando eu inspeciono o vocabulário de status de candidatura
    Então todos os status devem ter rótulo em português nas duas visões
    E nenhum rótulo de status deve repetir dentro da mesma visão

  # Regressão da entrega conjunta: a API passa a devolver `CanceledByCandidate` e o frontend
  # tem de reconhecê-lo, senão o badge exibe o identificador cru em vez de "Cancelada por você".
  Esquema do Cenário: reconhecer o status devolvido pela API
    Quando eu interpreto o status "<status>"
    Então o status interpretado deve ser "<esperado>"

    Exemplos:
      | status              | esperado            |
      | Pending             | Pending             |
      | Processing          | Processing          |
      | CanceledByCandidate | CanceledByCandidate |
      | Canceled            | Canceled            |
      | Encerrada           |                     |
      | cancelledbycandidate |                     |

  # O ato da empresa e o ato do candidato são status distintos, não uma coluna de autoria:
  # o recrutador precisa distinguir a desistência do cancelamento que ele próprio fez.
  Esquema do Cenário: rótulo do status conforme quem lê
    Quando eu leio o rótulo do status "<status>" na visão "<visao>"
    Então o rótulo do status deve ser "<rotulo>"

    Exemplos:
      | status              | visao     | rotulo                  |
      | Pending             | candidate | Recebida                |
      | Pending             | recruiter | Recebida                |
      | Processing          | candidate | Em análise              |
      | CanceledByCandidate | candidate | Cancelada por você      |
      | CanceledByCandidate | recruiter | Cancelada pelo candidato |
      | Canceled            | candidate | Cancelada               |
      | Canceled            | recruiter | Cancelada               |

  Esquema do Cenário: a ação de cancelar só existe enquanto a candidatura está em aberto
    Quando eu verifico se o candidato pode cancelar uma candidatura "<status>"
    Então o cancelamento pelo candidato deve estar "<disponibilidade>"

    Exemplos:
      | status              | disponibilidade |
      | Pending             | disponível      |
      | Processing          | disponível      |
      | Approved            | indisponível    |
      | Rejected            | indisponível    |
      | Finished            | indisponível    |
      | Canceled            | indisponível    |
      | CanceledByCandidate | indisponível    |
      | Timeout             | indisponível    |
      | Error               | indisponível    |
      | Encerrada           | indisponível    |

  # X3/X6: o ato do candidato é terminal e a empresa não o sobrepõe.
  Cenário: cancelamento pelo candidato é terminal e não é ato do recrutador
    Quando eu inspeciono o vocabulário de status de candidatura
    Então o status "CanceledByCandidate" não deve ter transição de saída
    E nenhuma transição do recrutador deve levar a "CanceledByCandidate"
