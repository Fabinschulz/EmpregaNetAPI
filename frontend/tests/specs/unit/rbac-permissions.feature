#language: pt

Funcionalidade: Capacidades de exclusão por papel (RBAC)
  Como sistema
  Quero resolver cada capacidade de exclusão pela política que o endpoint exige no backend
  Para que o botão "Excluir" só apareça a quem a API realmente autoriza

  Esquema do Cenário: excluir empresa exige a política Administrador
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico a capacidade "company.delete"
    Então a capacidade deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitida |
      | Recruiter | negada    |
      | Manager   | negada    |
      | Candidate | negada    |
      |           | negada    |

  Esquema do Cenário: excluir usuário exige a política Administrador
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico a capacidade "user.delete"
    Então a capacidade deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitida |
      | Recruiter | negada    |
      | Manager   | negada    |
      | Candidate | negada    |
      |           | negada    |

  Esquema do Cenário: excluir vaga exige a política Recrutamento
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico a capacidade "job.delete"
    Então a capacidade deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitida |
      | Recruiter | permitida |
      | Manager   | permitida |
      | Candidate | negada    |
      |           | negada    |

  Esquema do Cenário: excluir candidatura exige a política Recrutamento
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico a capacidade "jobApplication.delete"
    Então a capacidade deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitida |
      | Recruiter | permitida |
      | Manager   | permitida |
      | Candidate | negada    |
      |           | negada    |

  Esquema do Cenário: a checagem de capacidade não diferencia maiúsculas de minúsculas
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico a capacidade "job.delete"
    Então a capacidade deve ser "permitida"

    Exemplos:
      | papeis    |
      | recruiter |
      | RECRUITER |
