import { isJobViewable } from '@/features/candidaturas/domain';
import { Given, Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

function parseFlag(value: string): boolean | undefined {
  return value === 'ausente' ? undefined : value === 'true';
}

Given(
  'uma candidatura com isJobViewable {string} e isJobAvailable {string}',
  function (this: BusinessRulesWorld, viewable: string, available: string) {
    this.result = { isJobViewable: parseFlag(viewable), isJobAvailable: parseFlag(available) };
  }
);

Then('o link "Ver vaga" deve aparecer: {string}', function (this: BusinessRulesWorld, expected: string) {
  const flags = this.result as { isJobViewable?: boolean; isJobAvailable?: boolean };
  expect(isJobViewable(flags)).to.equal(expected === 'sim');
});
