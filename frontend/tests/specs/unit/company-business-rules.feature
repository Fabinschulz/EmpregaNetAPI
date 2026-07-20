#language: pt

Funcionalidade: Regras de negócio do formulário de empresa
  Como administrador
  Quero que o formulário de cadastro de empresa rejeite dados inválidos
  Para que a API nunca receba um CNPJ, telefone ou endereço fora do contrato exigido

  Contexto:
    Dado que tenho os dados válidos de uma empresa para o formulário

  Cenário: aceitar todos os campos quando estão válidos
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser aceitos

  Esquema do Cenário: rejeitar CNPJ que não tenha exatamente 14 dígitos
    Dado que o campo "cnpj" do formulário de empresa é "<cnpj>"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

    Exemplos:
      | cnpj              |
      | 123               |
      | 1234567890123456  |
      |                   |

  Esquema do Cenário: aceitar CNPJ com 14 dígitos, com ou sem máscara de pontuação
    Dado que o campo "cnpj" do formulário de empresa é "<cnpj>"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser aceitos

    Exemplos:
      | cnpj                |
      | 12345678000199      |
      | 12.345.678/0001-99  |

  Esquema do Cenário: rejeitar telefone fora do intervalo de 10 a 11 dígitos
    Dado que o campo "phone" do formulário de empresa é "<telefone>"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

    Exemplos:
      | telefone      |
      | 123           |
      | (11) 999999999999 |

  Cenário: rejeitar e-mail em formato inválido
    Dado que o campo "email" do formulário de empresa é "nao-e-um-email"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

  Cenário: rejeitar quando o tipo de atividade não é uma opção válida
    Dado que o campo "typeOfActivity" do formulário de empresa é "NaoSelecionado"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

  Esquema do Cenário: rejeitar CEP fora do formato 00000-000
    Dado que o campo "address.zipCode" do formulário de empresa é "<cep>"
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

    Exemplos:
      | cep      |
      | 123      |
      | abcde-123 |

  Esquema do Cenário: rejeitar quando um campo obrigatório do endereço está vazio
    Dado que o campo "<campo>" do formulário de empresa é ""
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser rejeitados

    Exemplos:
      | campo               |
      | address.street      |
      | address.number      |
      | address.neighborhood |
      | address.city        |

  Cenário: complemento de endereço é opcional
    Dado que o campo "address.complement" do formulário de empresa é ""
    Quando eu valido os dados do formulário de empresa
    Então os dados da empresa devem ser aceitos
