#language: pt

# Recarregar uma lista que não mudou não altera nada na tela, e o clique parece não ter funcionado.
# O aviso confirma a ação; o total é o que mostra que os dados são novos.

Funcionalidade: Aviso de lista atualizada
  Como usuário de uma listagem
  Quero um retorno claro ao clicar em Atualizar
  Para saber que a ação aconteceu mesmo quando os dados não mudaram

  Esquema do Cenário: informar o total no plural neutro de gênero
    Quando eu monto o aviso de lista atualizada com <total> em "<recurso>"
    Então o aviso deve ser "<esperado>"

    Exemplos:
      | total | recurso      | esperado                   |
      | 12    | vagas        | 12 vagas no total.         |
      | 3     | candidatos   | 3 candidatos no total.     |
      | 1     | empresas     | 1 empresas no total.       |
      | 45    | candidaturas | 45 candidaturas no total.  |
      | 7     | usuários     | 7 usuários no total.       |

  Cenário: lista vazia tem texto próprio em vez de "0"
    Quando eu monto o aviso de lista atualizada com 0 em "vagas"
    Então o aviso deve ser "Nenhum resultado em vagas."

  # A API pode omitir `totalItems`; nesse caso o aviso sai só com o título.
  Cenário: sem total, o aviso não deve ter descrição
    Quando eu monto o aviso de lista atualizada sem total em "vagas"
    Então o aviso não deve ter descrição

  Cenário: total negativo é dado inválido e não deve virar texto
    Quando eu monto o aviso de lista atualizada com -1 em "vagas"
    Então o aviso não deve ter descrição
