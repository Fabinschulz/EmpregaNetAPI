#language: pt

Funcionalidade: Regras de negócio do formulário de vaga
  Como recrutador
  Quero que o formulário de publicação de vaga rejeite dados inválidos
  Para que a API nunca receba uma vaga sem empresa, tipo ou salário válidos

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

  Cenário: rejeitar quando o tipo de vaga não é uma opção válida
    Dado que o campo "jobType" do formulário de vaga é "NaoSelecionado"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

  Esquema do Cenário: rejeitar salário negativo, não numérico ou vazio
    Dado que o campo "salary" do formulário de vaga é "<salario>"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser rejeitados

    Exemplos:
      | salario |
      | -100    |
      | abc     |
      |         |

  Esquema do Cenário: aceitar salário válido, incluindo zero
    Dado que o campo "salary" do formulário de vaga é "<salario>"
    Quando eu valido os dados do formulário de vaga
    Então os dados da vaga devem ser aceitos

    Exemplos:
      | salario  |
      | 0        |
      | 1500     |
      | 1500.50  |
