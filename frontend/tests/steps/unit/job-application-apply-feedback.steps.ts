import { applyFeedbackCopy } from '@/features/candidaturas/apply-feedback-copy';
import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

function feedbackText(world: BusinessRulesWorld): string {
  return world.result as string;
}

When('eu leio o feedback de candidatura enviada', function (this: BusinessRulesWorld) {
  this.result = `${applyFeedbackCopy.title} ${applyFeedbackCopy.description}`;
});

Then('o título do feedback deve ser {string}', function (esperado: string) {
  expect(applyFeedbackCopy.title).to.equal(esperado);
});

Then('o feedback deve indicar onde acompanhar a candidatura', function (this: BusinessRulesWorld) {
  expect(feedbackText(this), 'a confirmação precisa dizer onde o candidato acompanha o andamento').to.include(
    'Minhas candidaturas'
  );
});

Then('o feedback não deve prometer {string}', function (this: BusinessRulesWorld, termo: string) {
  expect(
    feedbackText(this).toLowerCase(),
    `o feedback voltou a mencionar "${termo}": ${feedbackText(this)}`
  ).to.not.include(termo.toLowerCase());
});
