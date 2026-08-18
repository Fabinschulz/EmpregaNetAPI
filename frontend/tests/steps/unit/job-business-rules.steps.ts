import { jobFormSchema, type JobFormValues } from '@/features/recrutamento/vagas/form/job-form-schema';
import { MAX_VOCABULARY_ITEMS_PER_JOB } from '@/shared/schema/job-vocabulary';
import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { setByPath } from '../../support/object-path';
import type { BusinessRulesWorld } from '../../support/world';

/** Dados de um formulário de vaga 100% válido — ponto de partida de cada cenário. */
function validJobFormData(): JobFormValues {
  return {
    companyId: '42',
    title: 'Operador(a) de Empilhadeira',
    summary: 'Centro de distribuição, 2º turno, com fretado.',
    description: 'Movimentação de cargas e conferência no armazém.',
    jobType: 'Clt',
    workModel: 'OnSite',
    workShift: 'SegundoTurno',
    experienceLevel: 'AteUmAno',
    area: 'Logistica',
    city: 'Extrema',
    state: 'MG',
    pcd: 'no',
    salaryDisclosure: 'disclosed',
    salaryMin: '2300',
    salaryMax: '2800',
    requirements: ['Ensino Médio completo', 'Empilhadeira'],
    benefits: ['Fretado', 'Cesta Básica']
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

Given(
  'que a lista {string} do formulário de vaga contém {string}',
  function (this: BusinessRulesWorld, campo: string, valor: string) {
    if (!this.data.job) {
      this.data.job = validJobFormData();
    }
    setByPath(this.data.job as Record<string, unknown>, campo, [valor]);
  }
);

Given('que a lista {string} do formulário de vaga está vazia', function (this: BusinessRulesWorld, campo: string) {
  if (!this.data.job) {
    this.data.job = validJobFormData();
  }
  setByPath(this.data.job as Record<string, unknown>, campo, []);
});

Given(
  'que a lista {string} do formulário de vaga excede o teto por vaga',
  function (this: BusinessRulesWorld, campo: string) {
    if (!this.data.job) {
      this.data.job = validJobFormData();
    }

    const excedente = Array.from({ length: MAX_VOCABULARY_ITEMS_PER_JOB + 1 }, (_, i) => `item-${i}`);
    setByPath(this.data.job as Record<string, unknown>, campo, excedente);
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
