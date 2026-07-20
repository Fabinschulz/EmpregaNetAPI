#language: pt

Funcionalidade: Controle de acesso a rotas por papel (RBAC)
  Como sistema
  Quero decidir se um usuário pode acessar uma rota com base nos seus papéis
  Para que áreas administrativas e de recrutamento fiquem restritas a quem tem permissão

  Esquema do Cenário: rotas públicas são sempre permitidas, com ou sem papel
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico o acesso à rota "<rota>"
    Então o acesso deve ser "permitido"

    Exemplos:
      | papeis    | rota       |
      |           | /login     |
      |           | /vagas     |
      |           | /vagas/123 |
      | Candidate | /vagas     |

  Esquema do Cenário: a área /admin é restrita a administradores
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico o acesso à rota "/admin/usuarios"
    Então o acesso deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitido |
      | Recruiter | negado    |
      | Manager   | negado    |
      | Candidate | negado    |
      |           | negado    |

  Esquema do Cenário: a área /recrutamento é restrita à equipe de recrutamento (Admin, Recruiter ou Manager)
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico o acesso à rota "/recrutamento/vagas"
    Então o acesso deve ser "<resultado>"

    Exemplos:
      | papeis    | resultado |
      | Admin     | permitido |
      | Recruiter | permitido |
      | Manager   | permitido |
      | Candidate | negado    |
      |           | negado    |

  Esquema do Cenário: a checagem de papel não diferencia maiúsculas de minúsculas
    Dado que o usuário tem os papéis "<papeis>"
    Quando eu verifico o acesso à rota "/admin/usuarios"
    Então o acesso deve ser "permitido"

    Exemplos:
      | papeis |
      | admin  |
      | ADMIN  |
