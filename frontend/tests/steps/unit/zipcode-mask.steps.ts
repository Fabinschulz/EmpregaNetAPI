import { When, Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import { isCompleteZipCode, maskZipCode } from '@/utils';
import type { BusinessRulesWorld } from '../../support/world';

When('eu aplico a máscara de CEP a {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = maskZipCode(entrada);
});

Then('o resultado da máscara de CEP deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});

When('eu verifico se o CEP {string} está completo', function (this: BusinessRulesWorld, cep: string) {
  this.result = isCompleteZipCode(cep);
});

Then('o CEP deve ser considerado {string}', function (this: BusinessRulesWorld, situacao: string) {
  expect(this.result).to.equal(situacao === 'completo');
});
