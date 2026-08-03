#language: pt

Funcionalidade: Regras de negócio do formulário de vaga
  Como recrutador
  Quero que o formulário de publicação de vaga rejeite dados inválidos
  Para que a API nunca receba uma vaga sem empresa, turno, localização ou salário coerente

  Contexto:
    Dado que tenho os dados válidos de uma vaga para o formulário

  Cenário: aceitar todos os campos quando estão válidos
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

  Cenário: rejeitar quando nenhuma empresa foi selecionada
    Dado que o campo "companyId" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: rejeitar quando o título está vazio
    Dado que o campo "title" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: rejeitar quando a descrição está vazia
    Dado que o campo "description" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  # Turno e experiência são obrigatórios porque são os filtros de maior peso no feed: vaga sem
  # eles fica invisível para quem busca por turno, que é a maioria neste mercado.
  Esquema do Cenário: rejeitar classificação fora do vocabulário
    Dado que o campo "<campo>" do formulário de vaga é "<valor>"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

    Exemplos:
      | campo           | valor          |
      | jobType         | NaoSelecionado |
      | jobType         |                |
      | workShift       | NaoSelecionado |
      | workShift       | Noturno        |
      | experienceLevel | Pleno          |
      | experienceLevel |                |
      | workModel       | Presencial     |
      | area            | Development    |

  # O índice 8 do enum de vínculo era "Remote" e ficou reservado ao virar modalidade (ADR 0006).
  Cenário: rejeitar o vínculo "Remote", que deixou de existir
    Dado que o campo "jobType" do formulário de vaga é "Remote"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  # "Júnior/Pleno/Sênior" saiu do domínio ao virar experiência medida em tempo (ADR 0008).
  Cenário: rejeitar a escala de senioridade antiga
    Dado que o campo "experienceLevel" do formulário de vaga é "MidLevel"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Esquema do Cenário: exigir cidade e estado da vaga
    Dado que o campo "<campo>" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

    Exemplos:
      | campo |
      | city  |
      | state |

  Cenário: rejeitar estado que não é uma UF brasileira
    Dado que o campo "state" do formulário de vaga é "XX"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Esquema do Cenário: rejeitar salário negativo ou não numérico
    Dado que o campo "salaryMin" do formulário de vaga é "<salario>"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

    Exemplos:
      | salario |
      | -100    |
      | abc     |

  Cenário: rejeitar teto salarial menor que o piso
    Dado que o campo "salaryMin" do formulário de vaga é "3000"
    E que o campo "salaryMax" do formulário de vaga é "2000"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: rejeitar salário divulgado sem nenhum valor informado
    Dado que o campo "salaryMin" do formulário de vaga é ""
    E que o campo "salaryMax" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: aceitar salário a combinar sem faixa informada
    Dado que o campo "salaryDisclosure" do formulário de vaga é "undisclosed"
    E que o campo "salaryMin" do formulário de vaga é ""
    E que o campo "salaryMax" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

  Esquema do Cenário: aceitar apenas um dos limites salariais
    Dado que o campo "<campo>" do formulário de vaga é ""
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

    Exemplos:
      | campo     |
      | salaryMin |
      | salaryMax |

  # Requisitos e benefícios são listas abertas servidas por GET /api/jobs/vocabulary. A
  # pertinência é validada pelo backend, que é a fonte única — replicar a lista no cliente só
  # para validar recriaria a duplicação que este desenho eliminou. O cliente ainda impõe o teto.
  Cenário: aceitar requisito fora da lista, deixando a validação para o backend
    Dado que a lista "requirements" do formulário de vaga contém "Requisito inexistente"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

  Cenário: rejeitar mais requisitos do que o teto por vaga
    Dado que a lista "requirements" do formulário de vaga excede o teto por vaga
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: rejeitar mais benefícios do que o teto por vaga
    Dado que a lista "benefits" do formulário de vaga excede o teto por vaga
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Cenário: aceitar vaga sem requisitos nem benefícios
    Dado que a lista "requirements" do formulário de vaga está vazia
    E que a lista "benefits" do formulário de vaga está vazia
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

  Esquema do Cenário: aceitar vaga afirmativa e vaga regular
    Dado que o campo "pcd" do formulário de vaga é "<valor>"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

    Exemplos:
      | valor |
      | no    |
      | yes   |
