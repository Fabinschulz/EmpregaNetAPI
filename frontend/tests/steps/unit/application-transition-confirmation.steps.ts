import {
  applicationTransitionDialogTitle,
  describeApplicationTransitionConfirmation
} from '@/features/candidaturas/application-transition-copy';
import { parseApplicationStatus } from '@/features/candidaturas/domain';
import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

When(
  'eu leio o título de confirmação da transição para {string} da candidatura de {string}',
  function (this: BusinessRulesWorld, statusAlvo: string, candidato: string) {
    const target = parseApplicationStatus(statusAlvo);
    expect(target, `status alvo desconhecido: ${statusAlvo}`).to.not.equal(null);
    this.result = applicationTransitionDialogTitle(target!, candidato);
  }
);

Then('o título de confirmação deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result as string).to.equal(esperado);
});

When(
  'eu leio a descrição de confirmação da transição de {string} para {string}',
  function (this: BusinessRulesWorld, statusAtual: string, statusAlvo: string) {
    const current = parseApplicationStatus(statusAtual);
    const target = parseApplicationStatus(statusAlvo);
    expect(target, `status alvo desconhecido: ${statusAlvo}`).to.not.equal(null);
    this.result = describeApplicationTransitionConfirmation(current, target!);
  }
);

Then('a descrição de confirmação deve dizer {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result as string).to.equal(esperado);
});
