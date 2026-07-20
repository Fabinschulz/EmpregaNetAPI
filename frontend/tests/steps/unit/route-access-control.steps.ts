import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { canAccessPath } from '@/utils';
import type { BusinessRulesWorld } from '../../support/world';

Given('que o usuário tem os papéis {string}', function (this: BusinessRulesWorld, papeis: string) {
  this.data.roles = papeis
    .split(',')
    .map((r) => r.trim())
    .filter(Boolean);
});

When('eu verifico o acesso à rota {string}', function (this: BusinessRulesWorld, rota: string) {
  const roles = (this.data.roles as string[] | undefined) ?? [];
  this.result = canAccessPath(rota, roles);
});

Then('o acesso deve ser {string}', function (this: BusinessRulesWorld, resultado: string) {
  expect(this.result).to.equal(resultado === 'permitido');
});
