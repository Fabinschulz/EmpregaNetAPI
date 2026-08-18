import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { isSessionValid, type Session } from '@/shared/auth/session';
import type { BusinessRulesWorld } from '../../support/world';

function sessionWith(token: string, exp: number | undefined): Session {
  return { token, roles: [], exp, username: null, email: null };
}

Given('que não há sessão', function (this: BusinessRulesWorld) {
  this.data.session = null;
});

Given(
  'que a sessão tem o token {string} e expira em {int} segundos',
  function (this: BusinessRulesWorld, token: string, segundos: number) {
    this.data.session = sessionWith(token, Math.floor(Date.now() / 1000) + segundos);
  }
);

Given('que a sessão tem o token {string} sem expiração', function (this: BusinessRulesWorld, token: string) {
  this.data.session = sessionWith(token, undefined);
});

When('eu verifico a validade da sessão', function (this: BusinessRulesWorld) {
  this.result = isSessionValid(this.data.session as Session | null);
});

Then('a sessão deve ser válida', function (this: BusinessRulesWorld) {
  expect(this.result).to.equal(true);
});

Then('a sessão deve ser inválida', function (this: BusinessRulesWorld) {
  expect(this.result).to.equal(false);
});
