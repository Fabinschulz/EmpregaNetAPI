import { When, Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import { isValidBrazilCellPhone, isValidBrazilPhone, maskBrazilPhone } from '@/utils';
import type { BusinessRulesWorld } from '../../support/world';

When('eu aplico a máscara de telefone a {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = maskBrazilPhone(entrada);
});

Then('o resultado da máscara deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});

When('eu valido o telefone {string}', function (this: BusinessRulesWorld, telefone: string) {
  this.result = isValidBrazilPhone(telefone);
});

When(
  'eu valido o celular {string} para cadastro de usuário',
  function (this: BusinessRulesWorld, telefone: string) {
    this.result = isValidBrazilCellPhone(telefone);
  }
);

Then('o telefone deve ser considerado {string}', function (this: BusinessRulesWorld, validade: string) {
  expect(this.result).to.equal(validade === 'válido');
});

Then('o celular deve ser considerado {string}', function (this: BusinessRulesWorld, validade: string) {
  expect(this.result).to.equal(validade === 'válido');
});
