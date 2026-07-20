import { When, Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import { normalizeTypeOfActivity, normalizeUf } from '@/features/admin/empresas/service/companies-schema';
import type { BusinessRulesWorld } from '../../support/world';

/** "(vazio)" na tabela do Gherkin representa string vazia — mais legível que uma célula em branco. */
function resolveExpected(esperado: string): string {
  return esperado === '(vazio)' ? '' : esperado;
}

When('eu normalizo o estado \\(texto\\) {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = normalizeUf(entrada);
});

When('eu normalizo o estado \\(número\\) {int}', function (this: BusinessRulesWorld, indice: number) {
  this.result = normalizeUf(indice);
});

Then('o estado normalizado deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(resolveExpected(esperado));
});

When('eu normalizo o tipo de atividade {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = normalizeTypeOfActivity(entrada);
});

Then('o tipo de atividade normalizado deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(resolveExpected(esperado));
});
