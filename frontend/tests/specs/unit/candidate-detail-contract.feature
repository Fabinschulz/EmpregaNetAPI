#language: pt

# `CandidateDetailViewModel` garante valor em id, username, email, userType, roles, createdAt,
# isDeleted e applications — o mapper preenche com '' / [] / objeto vazio quando o cadastro não tem
# o dado. Afrouxar esses campos "por precaução" esconderia o drift que o zod existe para denunciar
# (ADR 0009). Anulável só onde o candidato pode realmente não ter: telefone, foto, endereço, idade.

Funcionalidade: Contrato de resposta da ficha do candidato
  Como desenvolvedor do frontend
  Quero que o schema da ficha seja estrito na forma e tolerante na regra
  Para que um campo renomeado pela API falhe alto em vez de virar tela vazia

  Cenário: aceitar a ficha completa
    Quando eu leio a ficha do candidato
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "state" igual a "MG"
    E a ficha lida deve ter 3 candidaturas no total

  Esquema do Cenário: aceitar nulo onde o cadastro pode não ter o dado
    Quando eu leio a ficha do candidato com "<campo>" nulo
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "<campo>" nulo

    Exemplos:
      | campo          |
      | phoneNumber    |
      | profilePicture |
      | city           |
      | state          |
      | age            |
      | updatedAt      |

  Esquema do Cenário: aceitar ausência onde o cadastro pode não ter o dado
    Quando eu leio a ficha do candidato sem "<campo>"
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "<campo>" nulo

    Exemplos:
      | campo          |
      | phoneNumber    |
      | profilePicture |
      | city           |
      | age            |

  Esquema do Cenário: recusar a ausência de campo que a API sempre envia
    Quando eu leio a ficha do candidato sem "<campo>"
    Então a leitura da resposta deve falhar

    Exemplos:
      | campo        |
      | id           |
      | username     |
      | email        |
      | userType     |
      | roles        |
      | createdAt    |
      | isDeleted    |
      | applications |

  # O defeito que esta regra evita: se "applications" fosse renomeado, um schema tolerante deixaria
  # o parse passar e a tela diria "Nenhuma candidatura registrada" sem erro nenhum.
  Cenário: recusar o resumo de candidaturas sem as contagens
    Quando eu leio a ficha do candidato com o resumo de candidaturas renomeado
    Então a leitura da resposta deve falhar

  Cenário: aceitar nome e e-mail vazios, que é o que o mapper devolve
    Quando eu leio a ficha do candidato com "username" e "email" vazios
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "username" igual a ""

  Cenário: aparar espaços nas bordas dos textos
    Quando eu leio a ficha do candidato com "city" igual a "  Extrema  "
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "city" igual a "Extrema"

  Cenário: tratar texto só com espaços como ausência de dado
    Quando eu leio a ficha do candidato com "city" igual a "   "
    Então a leitura da resposta deve ter sucesso
    E a ficha lida deve ter "city" nulo

  Cenário: cair no identificador quando o cadastro não tem nome
    Quando eu leio a ficha do candidato com "username" e "email" vazios
    Então o nome exibível do candidato deve ser "#5"
