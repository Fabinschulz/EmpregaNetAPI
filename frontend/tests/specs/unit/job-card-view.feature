#language: pt

# `toJobFacts` e `toJobTags` traduzem o item do feed no que o cartão mostra. Os enums chegam com o
# sentinela `NaoSelecionado` quando a vaga não preencheu o campo, e o cartão não pode escrever isso
# na tela nem reservar espaço para um dado que não existe.

Funcionalidade: Dados exibidos no cartão da vaga
  Como desenvolvedor do frontend
  Quero que o cartão derive apenas o que a vaga informou
  Para que campo em branco não vire texto solto nem espaço vazio

  Cenário: montar a banda de dados de uma vaga completa
    Dado uma vaga do feed
    Quando eu monto a banda de dados do cartão
    Então os dados do cartão devem ser "location,salary,workModel,jobType"

  Esquema do Cenário: omitir o dado que a vaga não informou
    Dado uma vaga do feed
    E a vaga tem "<campo>" como "NaoSelecionado"
    Quando eu monto a banda de dados do cartão
    Então os dados do cartão não devem incluir "<chave>"

    Exemplos:
      | campo     | chave     |
      | workModel | workModel |
      | jobType   | jobType   |

  Cenário: nunca omitir o salário, que tem texto próprio para ausência
    Dado uma vaga do feed
    E a vaga não divulga o salário
    Quando eu monto a banda de dados do cartão
    Então os dados do cartão devem incluir "salary"
    E o rótulo de "salary" deve ser "A combinar"

  Cenário: descartar a UF sentinela em vez de a escrever no cartão
    Dado uma vaga do feed
    E a vaga tem a UF como "NaoSelecionado"
    Quando eu monto a banda de dados do cartão
    Então o rótulo de "location" deve ser "Extrema"

  Cenário: destacar o salário divulgado
    Dado uma vaga do feed
    Quando eu monto a banda de dados do cartão
    Então o dado "salary" deve estar destacado

  Cenário: não destacar salário a combinar
    Dado uma vaga do feed
    E a vaga não divulga o salário
    Quando eu monto a banda de dados do cartão
    Então o dado "salary" não deve estar destacado

  # A ordem é a da utilidade para quem procura: primeiro o que abre ou fecha a candidatura.
  Cenário: ordenar as etiquetas por utilidade
    Dado uma vaga do feed
    E a vaga é afirmativa para PcD
    Quando eu monto as etiquetas do cartão
    Então as etiquetas do cartão devem ser "Vaga para PcD,Sem experiência,1º turno,Logística,Empilhadeira,Fretado"

  Cenário: destacar apenas o que muda a decisão de quem lê
    Dado uma vaga do feed
    E a vaga é afirmativa para PcD
    Quando eu monto as etiquetas do cartão
    Então as etiquetas destacadas do cartão devem ser "Vaga para PcD,Sem experiência"

  Cenário: vaga sem característica nenhuma não gera etiqueta
    Dado uma vaga sem características
    Quando eu monto as etiquetas do cartão
    Então as etiquetas do cartão devem ser ""
