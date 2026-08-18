#language: pt

# O fluxo é: criar → editar a entidade recém-criada → (usuário decide) Voltar → listagem.
# O POST devolve o id no corpo, e só isso: o header `Location` não é exposto pelo CORS da API, e
# uma frase de sucesso com o número embutido seria texto de apresentação virando contrato.

Funcionalidade: Fluxo de navegação dos formulários de entidade
  Como usuário que cadastra entidades
  Quero continuar no formulário depois de salvar
  Para completar os dados sem ser jogado de volta para a listagem

  Cenário: a resposta de criação é o id da entidade criada
    Quando eu leio a resposta de criação com id 42
    Então a leitura da criação deve ter sucesso
    E o id da entidade criada deve ser 42

  # Regressão: se o contrato aceitasse uma criação sem id utilizável, o fluxo "criar → editar"
  # cairia numa rota `/undefined` em silêncio, em vez de falhar onde o defeito está.
  Cenário: resposta de criação vazia deve ser recusada
    Quando eu leio uma resposta de criação vazia
    Então a leitura da criação deve falhar

  Cenário: id zero ou negativo não identifica entidade nenhuma
    Quando eu leio a resposta de criação com id 0
    Então a leitura da criação deve falhar

  # Guarda contra reintroduzir o envelope: se alguém voltar a devolver `{ id, message }`, o parse
  # falha aqui em vez de a navegação receber um objeto onde espera um número.
  Cenário: o envelope antigo com mensagem deve ser recusado
    Quando eu leio uma resposta de criação com o corpo antigo
    Então a leitura da criação deve falhar

  Esquema do Cenário: cada entidade leva para a edição do que acabou de criar
    Quando eu resolvo a rota de detalhe de "<entidade>" para o id 7
    Então a rota resolvida deve ser "<rota>"

    Exemplos:
      | entidade | rota                    |
      | empresa  | /admin/empresas/7       |
      | vaga     | /recrutamento/vagas/7   |
      | usuario  | /admin/usuarios/7       |

  Esquema do Cenário: o botão Voltar aponta para a listagem da própria entidade
    Quando eu resolvo a rota de listagem de "<entidade>"
    Então a rota resolvida deve ser "<rota>"

    Exemplos:
      | entidade | rota                  |
      | empresa  | /admin/empresas       |
      | vaga     | /recrutamento/vagas   |
      | usuario  | /admin/usuarios       |
