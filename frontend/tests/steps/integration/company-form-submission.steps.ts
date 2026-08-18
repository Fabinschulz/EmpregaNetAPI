import { Given, Then, When, type DataTable } from '@cucumber/cucumber';
import { expect } from 'chai';
import { companyRequestSchema } from '@/features/admin/empresas/service/companies-request-schema';
import {
  companyFormSchema,
  companyFormToRequest,
  companyFormValuesFromResponse
} from '@/features/admin/empresas/form/company-form-schema';
import { companyResponseSchema } from '@/features/admin/empresas/service/companies-response-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

Given('que a API devolveu os dados desta empresa cadastrada:', function (this: BusinessRulesWorld, rawJson: string) {
  this.data.rawCompany = JSON.parse(rawJson);
});

When('eu carrego esses dados no formulário de edição de empresa', function (this: BusinessRulesWorld) {
  const dto = companyResponseSchema.parse(this.data.rawCompany);
  this.data.companyFormValues = companyFormValuesFromResponse(dto);
});

When('eu monto o payload de reenvio a partir do formulário, sem alterar nada', function (this: BusinessRulesWorld) {
  const formValues = companyFormSchema.parse(this.data.companyFormValues);
  this.result = companyFormToRequest(formValues);
});

Then('o payload de reenvio deve satisfazer o contrato de requisição', function (this: BusinessRulesWorld) {
  const parsed = companyRequestSchema.safeParse(this.result);
  const issues = parsed.success ? '' : parsed.error.issues.map((i) => `${i.path.join('.')}: ${i.message}`).join(' | ');
  expect(parsed.success, `o mapeamento produziu um corpo que a API recusaria — ${issues}`).to.equal(true);
});

Then('o payload de reenvio deve conter:', function (this: BusinessRulesWorld, table: DataTable) {
  const payload = this.result as Record<string, unknown>;
  for (const { campo, valor } of table.hashes()) {
    const actual = getByPath(payload, campo);
    expect(String(actual), `campo "${campo}"`).to.equal(valor);
  }
});

When('eu valido esses dados contra o contrato de leitura de empresa', function (this: BusinessRulesWorld) {
  this.data.companyParseResult = companyResponseSchema.safeParse(this.data.rawCompany);
});

Then(
  'a validação do contrato de empresa deve falhar no campo {string}',
  function (this: BusinessRulesWorld, campo: string) {
    const parsed = this.data.companyParseResult as ReturnType<typeof companyResponseSchema.safeParse>;
    expect(parsed.success, 'esperava que o contrato rejeitasse a resposta').to.equal(false);

    const paths = parsed.success ? [] : parsed.error.issues.map((issue) => issue.path.join('.'));
    expect(paths, `campos com erro: ${paths.join(', ')}`).to.include(campo);
  }
);

Then('o complemento do endereço no payload de reenvio deve ser nulo', function (this: BusinessRulesWorld) {
  const payload = this.result as { address: { complement: unknown } };
  expect(payload.address.complement).to.equal(null);
});
