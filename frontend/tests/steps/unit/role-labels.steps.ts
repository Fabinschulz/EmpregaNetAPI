import { Then, When } from '@cucumber/cucumber';
import { roleLabel } from '@/shared/utils/lib/user-types';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

When('eu peço o rótulo do papel {string}', function (this: BusinessRulesWorld, papel: string) {
  this.result = roleLabel(papel);
});

Then('o rótulo do papel deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});
