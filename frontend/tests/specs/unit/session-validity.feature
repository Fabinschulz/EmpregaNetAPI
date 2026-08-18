# language: pt
Funcionalidade: Validade da sessão lida do cookie
  Como sistema
  Quero decidir se uma requisição é tratada como autenticada
  Para que o gating de rota do proxy não dependa de expressão solta dentro do middleware

  Cenário: sessão ausente não é válida
    Dado que não há sessão
    Quando eu verifico a validade da sessão
    Então a sessão deve ser inválida

  Cenário: sessão sem token não é válida
    Dado que a sessão tem o token "" e expira em 3600 segundos
    Quando eu verifico a validade da sessão
    Então a sessão deve ser inválida

  Cenário: token dentro do prazo é válido
    Dado que a sessão tem o token "abc" e expira em 3600 segundos
    Quando eu verifico a validade da sessão
    Então a sessão deve ser válida

  Cenário: token já expirado não é válido
    Dado que a sessão tem o token "abc" e expira em -1 segundos
    Quando eu verifico a validade da sessão
    Então a sessão deve ser inválida

  Cenário: token sem exp é aceito
    # A API valida assinatura e prazo de verdade. Recusar aqui um token que o servidor aceitaria
    # produziria um logout que o usuário não entende.
    Dado que a sessão tem o token "abc" sem expiração
    Quando eu verifico a validade da sessão
    Então a sessão deve ser válida
