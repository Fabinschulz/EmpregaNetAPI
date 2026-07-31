#language: pt

Funcionalidade: Máscara de CEP e disparo da busca de endereço
  Como usuário preenchendo um endereço
  Quero que o CEP seja formatado enquanto digito
  Para que a busca automática do endereço só dispare quando o CEP estiver completo

  Esquema do Cenário: aplicar a máscara conforme os dígitos digitados
    Quando eu aplico a máscara de CEP a "<entrada>"
    Então o resultado da máscara de CEP deve ser "<esperado>"

    Exemplos:
      | entrada     | esperado  |
      |             |           |
      | 3           | 3         |
      | 37640       | 37640     |
      | 376409      | 37640-9   |
      | 37640970    | 37640-970 |
      | 37640-970   | 37640-970 |
      | 376409701234| 37640-970 |
      | abc37640970 | 37640-970 |

  Esquema do Cenário: só considerar o CEP completo com oito dígitos
    Quando eu verifico se o CEP "<cep>" está completo
    Então o CEP deve ser considerado "<situacao>"

    Exemplos:
      | cep       | situacao   |
      | 37640-970 | completo   |
      | 37640970  | completo   |
      | 3764097   | incompleto |
      | 376409701 | incompleto |
      |           | incompleto |
