#language: pt

Funcionalidade: Vocabulário do domínio de vagas
  Como pessoa usuária
  Quero ver rótulos em português em toda a aplicação
  Para nunca encontrar o nome cru de um enum do backend na tela

  Esquema do Cenário: todo valor do vocabulário tem rótulo em português
    Quando eu inspeciono o vocabulário "<vocabulario>"
    Então todos os valores devem ter rótulo
    E nenhum rótulo deve repetir

    Exemplos:
      | vocabulario |
      | jobType     |
      | workModel   |
      | shift       |
      | experience  |
      | area        |

  Esquema do Cenário: aceitar o nome do enum vindo da API
    Quando eu normalizo "<valor>" no vocabulário "<vocabulario>"
    Então o valor normalizado deve ser "<esperado>"

    Exemplos:
      | vocabulario | valor      | esperado   |
      | jobType     | Clt            | Clt            |
      | jobType     | clt            | Clt            |
      | workModel   | Remote         | Remote         |
      | shift       | SegundoTurno   | SegundoTurno   |
      | shift       | segundoturno   | SegundoTurno   |
      | experience  | SemExperiencia | SemExperiencia |
      | area        | Logistica      | Logistica      |

  # Endpoints anteriores ao feed devolvem o enum como inteiro; o feed devolve o nome.
  # A normalização precisa aceitar as duas formas.
  Esquema do Cenário: aceitar o índice do enum vindo de endpoints antigos
    Quando eu normalizo "<indice>" no vocabulário "<vocabulario>"
    Então o valor normalizado deve ser "<esperado>"

    Exemplos:
      | vocabulario | indice | esperado  |
      | jobType     | 1      | FullTime       |
      | jobType     | 9      | Clt            |
      | jobType     | 10     | Pj             |
      | workModel   | 3      | Remote         |
      | shift       | 3      | SegundoTurno   |
      | experience  | 1      | SemExperiencia |
      | area        | 2      | Logistica      |

  # O índice 8 era `Remote` e ficou reservado quando a modalidade saiu do enum de vínculo
  # (ADR 0006). Se voltar a resolver para alguma coisa, dado antigo passa a significar
  # outro vínculo em silêncio - exatamente o que a reserva existe para impedir.
  Cenário: o índice 8 do vínculo continua reservado
    Quando eu normalizo "8" no vocabulário "jobType"
    Então o valor normalizado deve ser ""

  Esquema do Cenário: descartar valores desconhecidos
    Quando eu normalizo "<valor>" no vocabulário "<vocabulario>"
    Então o valor normalizado deve ser ""

    Exemplos:
      | vocabulario | valor          |
      | jobType     | NaoSelecionado |
      | jobType     | Remote         |
      | jobType     | 99             |
      | workModel   | Presencial     |
      | shift       | Noturno        |
      | experience  | Pleno          |
      | area        | Development    |

  # 'Development' era area válida no vocabulário de software house; a taxonomia industrial não
  # o tem, e um link antigo com ?area=Development deve ser descartado, não quebrar o feed.
  Cenário: as faixas salariais cobrem a escala sem deixar buraco
    Quando eu inspeciono as faixas salariais
    Então as faixas devem ser contínuas
    E a primeira faixa não deve ter piso
    E a última faixa não deve ter teto
