#language: pt

Funcionalidade: Máscara e validação de CPF
  Como usuário do sistema
  Quero que o CPF seja formatado enquanto digito e validado antes de enviar
  Para que a máscara não gere duplicidade e um CPF inválido não chegue à API

  Esquema do Cenário: aplicar a máscara conforme os dígitos digitados
    Quando eu aplico a máscara de CPF a "<entrada>"
    Então o resultado da máscara de CPF deve ser "<esperado>"

    Exemplos:
      | entrada         | esperado        |
      |                 |                 |
      | 5               | 5               |
      | 529             | 529             |
      | 5299            | 529.9           |
      | 529982          | 529.982         |
      | 5299822         | 529.982.2       |
      | 529982247       | 529.982.247     |
      | 52998224725     | 529.982.247-25  |
      | 529.982.247-25  | 529.982.247-25  |
      | 5299822472512   | 529.982.247-25  |

  Esquema do Cenário: validar o CPF pelos dígitos verificadores
    Quando eu valido o CPF "<cpf>"
    Então o CPF deve ser considerado "<validade>"

    Exemplos:
      | cpf             | validade |
      | 52998224725     | válido   |
      | 529.982.247-25  | válido   |
      | 52998224726     | inválido |
      | 1234567890      | inválido |
      | 11111111111     | inválido |
      |                 | inválido |

  Esquema do Cenário: aceitar apenas CPF ou e-mail como identificador de login
    Quando eu valido o identificador de login "<identificador>"
    Então o identificador deve ser considerado "<validade>"

    Exemplos:
      | identificador       | validade |
      | usuario@email.com   | válido   |
      | 529.982.247-25      | válido   |
      | 52998224725         | válido   |
      | candidato1          | inválido |
      | admin               | inválido |
      | 52998224726         | inválido |
      | usuario@            | inválido |
      | @email.com          | inválido |
      |                     | inválido |

  # O CPF é definido uma vez, no cadastro. Deixá-lo fora da atualização não é só regra cadastral:
  # um endpoint autenticado que aceitasse CPF e respondesse "já existe" permitiria descobrir, um
  # palpite por vez, quais CPFs estão na base.
  Cenário: o CPF não faz parte da atualização de perfil
    Quando eu monto a atualização de perfil tentando enviar o CPF "529.982.247-25"
    Então o corpo enviado não deve conter o campo "cpf"
    E o corpo enviado deve conter o campo "phoneNumber"

  Cenário: o CPF faz parte do cadastro
    Quando eu monto o cadastro com o CPF "529.982.247-25"
    Então o corpo enviado deve conter o campo "cpf" com o valor "52998224725"
