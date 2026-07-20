import { companyFormSchema, type CompanyFormValues } from '@/features/admin/empresas/service/companies-schema';
import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { setByPath } from '../../support/object-path';
import type { BusinessRulesWorld } from '../../support/world';

/** Dados de um formulário de empresa 100% válido - ponto de partida de cada cenário. */
function validCompanyFormData(): CompanyFormValues {
  return {
    companyName: 'Empresa Exemplo Ltda',
    cnpj: '12345678000199',
    email: 'contato@empresa-exemplo.com',
    phone: '11988887777',
    typeOfActivity: 'Industry',
    address: {
      street: 'Rua das Flores',
      number: '100',
      complement: 'Sala 4',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '01310-100'
    }
  };
}

Given('que tenho os dados válidos de uma empresa para o formulário', function (this: BusinessRulesWorld) {
  this.data.company = validCompanyFormData();
});

Given(
  'que o campo {string} do formulário de empresa é {string}',
  function (this: BusinessRulesWorld, campo: string, valor: string) {
    if (!this.data.company) {
      this.data.company = validCompanyFormData();
    }
    setByPath(this.data.company as Record<string, unknown>, campo, valor);
  }
);

When('eu valido os dados do formulário de empresa', function (this: BusinessRulesWorld) {
  this.result = companyFormSchema.safeParse(this.data.company).success;
});

Then('os dados da empresa devem ser aceitos', function (this: BusinessRulesWorld) {
  expect(this.result, 'esperava que o schema aceitasse os dados, mas rejeitou').to.equal(true);
});

Then('os dados da empresa devem ser rejeitados', function (this: BusinessRulesWorld) {
  expect(this.result, 'esperava que o schema rejeitasse os dados, mas aceitou').to.equal(false);
});
