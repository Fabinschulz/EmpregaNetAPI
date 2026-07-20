import { Given, Then, When, type DataTable } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  companyFormSchema,
  companyFormToApiPayload,
  companyFormValuesFromDto,
  companySchema
} from '@/features/admin/empresas/service/companies-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

Given('que a API devolveu os dados desta empresa cadastrada:', function (this: BusinessRulesWorld, rawJson: string) {
  this.data.rawCompany = JSON.parse(rawJson);
});

When('eu carrego esses dados no formulário de edição de empresa', function (this: BusinessRulesWorld) {
  const dto = companySchema.parse(this.data.rawCompany);
  this.data.companyFormValues = companyFormValuesFromDto(dto);
});

When(
  'eu monto o payload de reenvio a partir do formulário, sem alterar nada',
  function (this: BusinessRulesWorld) {
    const formValues = companyFormSchema.parse(this.data.companyFormValues);
    this.result = companyFormToApiPayload(formValues);
  }
);

Then('o payload de reenvio deve conter:', function (this: BusinessRulesWorld, table: DataTable) {
  const payload = this.result as Record<string, unknown>;
  for (const { campo, valor } of table.hashes()) {
    const actual = getByPath(payload, campo);
    expect(String(actual), `campo "${campo}"`).to.equal(valor);
  }
});

Then('o complemento do endereço no payload de reenvio deve ser nulo', function (this: BusinessRulesWorld) {
  const payload = this.result as { address: { complement: unknown } };
  expect(payload.address.complement).to.equal(null);
});
