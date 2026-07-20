import { Given, Then, When, type DataTable } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  jobFormSchema,
  jobFormToApiPayload,
  jobFormValuesFromDto,
  jobSchema
} from '@/features/recrutamento/vagas/service/jobs-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

Given('que a API devolveu os dados desta vaga cadastrada:', function (this: BusinessRulesWorld, rawJson: string) {
  this.data.rawJob = JSON.parse(rawJson);
});

When('eu carrego esses dados no formulário de edição de vaga', function (this: BusinessRulesWorld) {
  const dto = jobSchema.parse(this.data.rawJob);
  this.data.jobFormValues = jobFormValuesFromDto(dto);
});

When(
  'eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada',
  function (this: BusinessRulesWorld) {
    const formValues = jobFormSchema.parse(this.data.jobFormValues);
    this.result = jobFormToApiPayload(formValues);
  }
);

Then('o payload de reenvio da vaga deve conter:', function (this: BusinessRulesWorld, table: DataTable) {
  const payload = this.result as Record<string, unknown>;
  for (const { campo, valor } of table.hashes()) {
    const actual = getByPath(payload, campo);
    expect(String(actual), `campo "${campo}"`).to.equal(valor);
  }
});

Then(
  'o campo {string} do payload de reenvio deve ser do tipo número',
  function (this: BusinessRulesWorld, campo: string) {
    const payload = this.result as Record<string, unknown>;
    expect(getByPath(payload, campo), `campo "${campo}"`).to.be.a('number');
  }
);
