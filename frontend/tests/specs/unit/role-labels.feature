#language: pt

# Os papéis do Identity chegam da API em inglês (`Candidate`, `Recruiter`, `Manager`, `Admin`) porque
# são identificadores de contrato usados no RBAC. Traduzir na origem quebraria a comparação de
# acesso, então o rótulo pt-BR é responsabilidade do frontend.

Funcionalidade: Rótulo pt-BR dos papéis de acesso
  Como usuário do backoffice
  Quero ver os papéis em português
  Para não encontrar nome técnico em inglês na interface

  Esquema do Cenário: traduzir os papéis do sistema
    Quando eu peço o rótulo do papel "<papel>"
    Então o rótulo do papel deve ser "<esperado>"

    Exemplos:
      | papel     | esperado      |
      | Candidate | Candidato     |
      | Recruiter | Recrutador    |
      | Manager   | Gestor        |
      | Admin     | Administrador |

  Cenário: aceitar o papel em qualquer caixa
    Quando eu peço o rótulo do papel "candidate"
    Então o rótulo do papel deve ser "Candidato"

  # Melhor o nome técnico do que um rótulo inventado: quem lê consegue reportar o que viu.
  Cenário: devolver o papel desconhecido como veio
    Quando eu peço o rótulo do papel "Interviewer"
    Então o rótulo do papel deve ser "Interviewer"

  Cenário: papel ausente não deve render texto vazio
    Quando eu peço o rótulo do papel ""
    Então o rótulo do papel deve ser "-"
