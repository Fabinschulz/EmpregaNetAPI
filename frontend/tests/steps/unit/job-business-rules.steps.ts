import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { jobFormSchema, type JobFormValues } from '@/features/recrutamento/vagas/service/jobs-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { setByPath } from '../../support/object-path';

/** Dados de um formulário de vaga 100% válido — ponto de partida de cada cenário. */
function validJobFormData(): JobFormValues {
  return {
    companyId: '42',
    title: 'Desenvolvedor(a) Full Stack',
    description: 'Vaga para atuar no time de plataforma.',
    jobType: 'FullTime',
    salary: '5000'
  };
}

Given('que tenho os dados válidos de uma vaga para o formulário', function (this: BusinessRulesWorld) {
  this.data.job = validJobFormData();
});

Given(
  'que o campo {string} do formulário de vaga é {string}',
  function (this: BusinessRulesWorld, campo: string, valor: string) {
    if (!this.data.job) {
      this.data.job = validJobFormData();
    }
    setByPath(this.data.job as Record<string, unknown>, campo, valor);
  }
);

When('eu valido os dados do formulário de vaga', function (this: BusinessRulesWorld) {
  this.result = jobFormSchema.safeParse(this.data.job).success;
});

Then('os dados da vaga devem ser aceitos', function (this: BusinessRulesWorld) {
  expect(this.result, 'esperava que o schema aceitasse os dados, mas rejeitou').to.equal(true);
});

Then('os dados da vaga devem ser rejeitados', function (this: BusinessRulesWorld) {
  expect(this.result, 'esperava que o schema rejeitasse os dados, mas aceitou').to.equal(false);
});
