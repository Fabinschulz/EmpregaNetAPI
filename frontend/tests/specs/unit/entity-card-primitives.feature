#language: pt

# Lógica pura dos primitivos compartilhados pelos cartões de vaga e de candidato
# (shared/components/ui/molecules/entity-card). São os dois pontos onde um dado cru do backend
# vira texto na tela sem passar por mais ninguém.

Funcionalidade: Primitivos do cartão de entidade
  Como desenvolvedor do frontend
  Quero que etiquetas e iniciais tratem dado cru de forma previsível
  Para que o cartão não repita, não mostre vazio e não invente sigla

  Esquema do Cenário: descartar rótulo vazio ao montar etiquetas
    Quando eu monto etiquetas a partir de "<entrada>"
    Então as etiquetas devem ser "<esperado>"

    Exemplos:
      | entrada                     | esperado          |
      | Empilhadeira,Fretado        | Empilhadeira,Fretado |
      | Empilhadeira,,Fretado       | Empilhadeira,Fretado |
      | Empilhadeira,   ,Fretado    | Empilhadeira,Fretado |
      |                             |                   |
      |   Empilhadeira  ,  Fretado  | Empilhadeira,Fretado |

  # Requisitos e benefícios são texto livre digitado pelo recrutador: "CNH B" e "cnh b" são o mesmo
  # requisito, e mostrar os dois no mesmo cartão parece defeito.
  Esquema do Cenário: remover repetição ignorando caixa
    Quando eu monto etiquetas a partir de "<entrada>"
    Então as etiquetas devem ser "<esperado>"

    Exemplos:
      | entrada             | esperado |
      | CNH B,cnh b         | CNH B    |
      | CNH B,CNH B         | CNH B    |
      | CNH B,  cnh b       | CNH B    |
      | CNH B,CNH C         | CNH B,CNH C |

  Cenário: a primeira ocorrência é a que fica
    Quando eu monto etiquetas a partir de "cnh b,CNH B"
    Então as etiquetas devem ser "cnh b"

  Esquema do Cenário: derivar as iniciais de uma empresa ou pessoa
    Quando eu peço as iniciais de "<nome>"
    Então as iniciais devem ser "<esperado>"

    Exemplos:
      | nome                     | esperado |
      | Freetech Inovation Ltda  | FI       |
      | Acme                     | A        |
      | Maria da Silva           | MS       |
      | Transportes e Logistica  | TL       |
      | Comercial S/A            | C        |
      | QA_E2E_Tester            | QE       |
      | acme logistica           | AL       |

  # Sem isto o avatar renderizaria uma caixa vazia, que parece falha de carregamento.
  Esquema do Cenário: usar interrogação quando não sobra palavra utilizável
    Quando eu peço as iniciais de "<nome>"
    Então as iniciais devem ser "?"

    Exemplos:
      | nome  |
      |       |
      | Ltda  |
      | de da |
      | ---   |
