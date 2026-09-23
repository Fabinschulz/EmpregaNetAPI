#language: pt

# Achado de QA: na listagem geral e na lista de candidatos por vaga, toda transição de status do
# processo seletivo disparava `PUT /api/jobapplications/{id}` no próprio clique, sem chance de
# desistir — mesmo as não-destrutivas ("Iniciar análise", "Aprovar", "Concluir"). A confirmação
# passa a valer para toda transição, e o texto precisa nomear o candidato e o "de/para" do status
# para quem aprova saber exatamente o que está confirmando.

Funcionalidade: Confirmação antes de transicionar o status da candidatura
  Como recrutador
  Quero que toda mudança de status peça confirmação com o nome do candidato e o "de/para"
  Para não avançar o processo seletivo por um clique que não dá chance de desistir

  Esquema do Cenário: o título nomeia a ação e o candidato
    Quando eu leio o título de confirmação da transição para "<status alvo>" da candidatura de "<candidato>"
    Então o título de confirmação deve ser "<titulo>"

    Exemplos:
      | status alvo | candidato       | titulo                                              |
      | Processing  | Ana Souza       | Iniciar a análise da candidatura de Ana Souza?      |
      | Approved    | Ana Souza       | Aprovar a candidatura de Ana Souza?                 |
      | Rejected    | Ana Souza       | Reprovar a candidatura de Ana Souza?                |
      | Canceled    | Ana Souza       | Cancelar a candidatura de Ana Souza?                |
      | Finished    | Ana Souza       | Concluir a candidatura de Ana Souza?                |

  Esquema do Cenário: a descrição diz de onde para onde o status vai mudar
    Quando eu leio a descrição de confirmação da transição de "<status atual>" para "<status alvo>"
    Então a descrição de confirmação deve dizer "<descricao>"

    Exemplos:
      | status atual | status alvo | descricao                                              |
      | Processing   | Approved    | Isso moverá o status de 'Em análise' para 'Aprovada'.  |
      | Processing   | Rejected    | Isso moverá o status de 'Em análise' para 'Reprovada'. |
      | Pending      | Processing  | Isso moverá o status de 'Recebida' para 'Em análise'.  |
      | Approved     | Finished    | Isso moverá o status de 'Aprovada' para 'Concluída'.   |
