import { Then, When } from '@cucumber/cucumber';
import { refreshedListMessage } from '@/shared/utils/format/refreshed-list-message';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

When(
  'eu monto o aviso de lista atualizada com {int} em {string}',
  function (this: BusinessRulesWorld, total: number, recurso: string) {
    this.result = refreshedListMessage(total, recurso);
  }
);

When(
  'eu monto o aviso de lista atualizada sem total em {string}',
  function (this: BusinessRulesWorld, recurso: string) {
    this.result = refreshedListMessage(undefined, recurso);
  }
);

Then('o aviso deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});

Then('o aviso não deve ter descrição', function (this: BusinessRulesWorld) {
  expect(this.result).to.equal(undefined);
});
