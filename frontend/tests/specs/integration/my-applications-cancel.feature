#language: pt

# Fluxo de "Minhas candidaturas" ao cancelar: da regra que decide se a ação aparece até o
# pedido que sai para a API e a leitura da resposta. O cancelamento é terminal, então a
# confirmação faz parte do comportamento — não é enfeite de UI.

Funcionalidade: Cancelar a própria candidatura
  Como candidato
  Quero desistir de uma candidatura ainda em aberto, com confirmação explícita
  Para não sair do processo por engano nem ficar preso a ele

  Cenário: cancelar uma candidatura em análise
    Dado que a minha candidatura #12 está com o status "Processing"
    Quando eu abro as ações da candidatura
    Então a ação "Cancelar candidatura" deve estar disponível
    Quando eu escolho cancelar a candidatura
    Então a confirmação de cancelamento deve estar aberta
    E a confirmação deve avisar que a ação não tem retorno
    E o botão que recusa a confirmação não deve se chamar "Cancelar"
    Quando eu confirmo o cancelamento
    Então a API deve ter recebido "PUT /api/jobapplications/12/cancel"
    E o pedido de cancelamento não deve ter corpo
    E a candidatura devolvida deve estar com o status "CanceledByCandidate"
    E o rótulo do status na minha lista deve ser "Cancelada por você"
    E a confirmação de cancelamento deve estar fechada

  # CA-12: abandonar a confirmação não altera nada — nem no cliente, nem no servidor.
  Cenário: abandonar a confirmação não cancela
    Dado que a minha candidatura #12 está com o status "Processing"
    Quando eu escolho cancelar a candidatura
    E eu abandono a confirmação
    Então a API não deve ter sido chamada
    E a confirmação de cancelamento deve estar fechada
    E a candidatura deve continuar com o status "Processing"

  # CA-11: a ação não é oferecida a partir de Aprovada nem em estado final. O backend recusa
  # o mesmo caso — a ausência na tela é conveniência, não a defesa.
  Esquema do Cenário: candidatura fora de Recebida/Em análise não oferece cancelamento
    Dado que a minha candidatura #12 está com o status "<status>"
    Quando eu abro as ações da candidatura
    Então a ação "Cancelar candidatura" não deve estar disponível

    Exemplos:
      | status              |
      | Approved            |
      | Rejected            |
      | Finished            |
      | Canceled            |
      | CanceledByCandidate |
      | Timeout             |
      | Error               |

  Cenário: candidatura recebida também pode ser cancelada
    Dado que a minha candidatura #12 está com o status "Pending"
    Quando eu abro as ações da candidatura
    Então a ação "Cancelar candidatura" deve estar disponível

  # A API é a fonte de verdade: se ela recusar o estado, o cliente não inventa sucesso.
  Cenário: recusa da API não altera a candidatura na tela
    Dado que a minha candidatura #12 está com o status "Processing"
    E que a API vai recusar o cancelamento com o código 400
    Quando eu escolho cancelar a candidatura
    E eu confirmo o cancelamento
    Então o cancelamento deve ter falhado
    E a candidatura deve continuar com o status "Processing"

  Cenário: a ação continua ao lado de "Ver vaga"
    Dado que a minha candidatura #12 está com o status "Processing"
    Quando eu abro as ações da candidatura
    Então a ação "Ver vaga" deve estar disponível
