#language: pt

Funcionalidade: Máscara e validação de telefone brasileiro
  Como usuário do sistema
  Quero que o telefone seja formatado enquanto digito e validado ao salvar
  Para que eu não consiga enviar um número inválido para a API

  Esquema do Cenário: aplicar a máscara conforme os dígitos digitados
    Quando eu aplico a máscara de telefone a "<entrada>"
    Então o resultado da máscara deve ser "<esperado>"

    Exemplos:
      | entrada          | esperado         |
      |                  |                  |
      | 1                | (1               |
      | 11               | (11              |
      | 119               | (11) 9          |
      | 1198888          | (11) 9888-8      |
      | 1198888777       | (11) 9888-8777   |
      | 11988887777      | (11) 98888-7777  |
      | (11) 98888-7777  | (11) 98888-7777  |
      | 119888877771234  | (11) 98888-7777  |

  Esquema do Cenário: validar telefone fixo ou celular
    Quando eu valido o telefone "<telefone>"
    Então o telefone deve ser considerado "<validade>"

    Exemplos:
      | telefone         | validade |
      | (11) 3888-7777   | válido   |
      | (11) 98888-7777  | válido   |
      | (01) 98888-7777  | inválido |
      | (11) 88888-7777  | inválido |
      | (11) 11111-1111  | inválido |
      | 123              | inválido |
      |                  | inválido |

  Esquema do Cenário: exigir celular (DDD + 9 dígitos) no cadastro de usuário
    Quando eu valido o celular "<telefone>" para cadastro de usuário
    Então o celular deve ser considerado "<validade>"

    Exemplos:
      | telefone         | validade |
      | (11) 98888-7777  | válido   |
      | (11) 3888-7777   | inválido |
      |                  | inválido |
