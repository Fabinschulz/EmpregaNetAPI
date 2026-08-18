#language: pt

# O backend mapeia `Username = user.UserName ?? string.Empty` e `Email = user.Email ?? string.Empty`
# (UserViewModel.cs), então string vazia e campo ausente são respostas legítimas — não erros de
# contrato. Regra de entrada ("mínimo 3 caracteres") pertence a registerSchema/profileFormSchema.

Funcionalidade: Contrato de resposta de usuário
  Como desenvolvedor do frontend
  Quero que o schema de resposta valide apenas a forma do payload
  Para que um dado incompleto vindo da API não derrube a tela inteira

  Cenário: aceitar usuário sem username nem e-mail
    Quando eu leio a resposta de usuário com "username" vazio e "email" vazio
    Então a leitura da resposta deve ter sucesso
    E o usuário lido deve ter "username" igual a ""
    E o usuário lido deve ter "email" igual a ""

  Esquema do Cenário: normalizar campos de texto nulos para vazio
    Quando eu leio a resposta de usuário com "<campo>" nulo
    Então a leitura da resposta deve ter sucesso
    E o usuário lido deve ter "<campo>" igual a ""

    Exemplos:
      | campo    |
      | username |
      | email    |
      | userType |

  Cenário: preservar nulo nas datas em que ele significa "não aconteceu"
    Quando eu leio a resposta de usuário sem "updatedAt" e sem "deletedAt"
    Então a leitura da resposta deve ter sucesso
    E o usuário lido deve ter "updatedAt" nulo
    E o usuário lido deve ter "deletedAt" nulo

  Cenário: assumir perfis vazios e não-excluído quando a API omite
    Quando eu leio a resposta de usuário sem "roles" e sem "isDeleted"
    Então a leitura da resposta deve ter sucesso
    E o usuário lido deve ter "roles" vazio
    E o usuário lido não deve estar excluído

  Cenário: rejeitar payload sem o identificador
    Quando eu leio a resposta de usuário sem "id"
    Então a leitura da resposta deve falhar

  Cenário: uma listagem não deve quebrar por causa de um registro incompleto
    Quando eu leio uma listagem de usuários em que o segundo registro tem "username" vazio
    Então a leitura da resposta deve ter sucesso
    E a listagem lida deve conter 3 usuários
