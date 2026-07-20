#language: pt

Funcionalidade: Fluxo completo do formulário de empresa (leitura da API até reenvio)
  Como sistema
  Quero que os dados de uma empresa sobrevivam intactos ao percurso completo
  API -> tela de edição -> reenvio para a API
  Para nunca mais reintroduzir a divergência de nomes de campo entre o
  formulário e o contrato do backend (bug já corrigido nesta base de código)

  Cenário: reabrir uma empresa cadastrada e reenviar sem alterações deve reproduzir os mesmos dados
    Dado que a API devolveu os dados desta empresa cadastrada:
      """
      {
        "id": 7,
        "companyName": "Empresa Exemplo Ltda",
        "registrationNumber": "12.345.678/0001-99",
        "email": "contato@empresa-exemplo.com",
        "phone": "(11) 98888-7777",
        "typeOfActivity": "Indústria",
        "address": {
          "street": "Rua das Flores",
          "number": "100",
          "complement": "Sala 4",
          "neighborhood": "Centro",
          "city": "São Paulo",
          "state": 25,
          "zipCode": "01310-100"
        }
      }
      """
    Quando eu carrego esses dados no formulário de edição de empresa
    E eu monto o payload de reenvio a partir do formulário, sem alterar nada
    Então o payload de reenvio deve conter:
      | campo                  | valor                       |
      | companyName            | Empresa Exemplo Ltda        |
      | cnpj                   | 12345678000199              |
      | email                  | contato@empresa-exemplo.com |
      | phone                  | 11988887777                 |
      | typeOfActivity         | Industry                    |
      | address.street         | Rua das Flores              |
      | address.number         | 100                         |
      | address.complement     | Sala 4                      |
      | address.neighborhood   | Centro                      |
      | address.city           | São Paulo                   |
      | address.state          | SP                          |
      | address.zipCode        | 01310-100                   |

  Cenário: complemento de endereço ausente na API deve virar null no reenvio, não uma string vazia
    Dado que a API devolveu os dados desta empresa cadastrada:
      """
      {
        "id": 8,
        "companyName": "Outra Empresa Ltda",
        "registrationNumber": "12345678000199",
        "email": "outra@empresa.com",
        "phone": "1133334444",
        "typeOfActivity": "services",
        "address": {
          "street": "Av. Central",
          "number": "500",
          "complement": null,
          "neighborhood": "Jardins",
          "city": "Belo Horizonte",
          "state": "MG",
          "zipCode": "30130-000"
        }
      }
      """
    Quando eu carrego esses dados no formulário de edição de empresa
    E eu monto o payload de reenvio a partir do formulário, sem alterar nada
    Então o complemento do endereço no payload de reenvio deve ser nulo
