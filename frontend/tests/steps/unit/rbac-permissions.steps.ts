import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { can, type Permission } from '@/shared/utils';
import type { BusinessRulesWorld } from '../../support/world';

// O step "Dado que o usuário tem os papéis {string}" é reaproveitado de
// route-access-control.steps.ts (definições de step são globais no Cucumber).

When('eu verifico a capacidade {string}', function (this: BusinessRulesWorld, capacidade: string) {
  const roles = (this.data.roles as string[] | undefined) ?? [];
  this.result = can(roles, capacidade as Permission);
});

Then('a capacidade deve ser {string}', function (this: BusinessRulesWorld, resultado: string) {
  expect(this.result).to.equal(resultado === 'permitida');
});
