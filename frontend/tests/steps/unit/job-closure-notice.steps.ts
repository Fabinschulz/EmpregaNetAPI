import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { JOB_STATUSES, jobStatusLabel, type JobStatus } from '@/features/recrutamento/vagas/domain';
import { summarizeJobClosure, type JobClosureSummary } from '@/features/vagas/detail/job-closure';
import { formatSalaryRange } from '@/shared/utils';
import type { BusinessRulesWorld } from '../../support/world';

function toStatus(value: string): JobStatus {
  const status = JOB_STATUSES.find((candidate) => candidate === value);
  if (!status) throw new Error(`Status desconhecido: ${value}`);
  return status;
}

function summary(world: BusinessRulesWorld): JobClosureSummary {
  return world.result as JobClosureSummary;
}

When('eu resumo o encerramento da vaga com status {string}', function (this: BusinessRulesWorld, status: string) {
  this.result = summarizeJobClosure({ status: toStatus(status), publishedAt: null, closedAt: null });
});

When(
  'eu resumo o encerramento da vaga publicada em {string} e encerrada em {string}',
  function (this: BusinessRulesWorld, publishedAt: string, closedAt: string) {
    this.result = summarizeJobClosure({ status: 'ClosedByFulfillment', publishedAt, closedAt });
  }
);

When(
  'eu resumo o encerramento da vaga publicada em {string} e sem data de encerramento',
  function (this: BusinessRulesWorld, publishedAt: string) {
    this.result = summarizeJobClosure({ status: 'ClosedManually', publishedAt, closedAt: null });
  }
);

When(
  'eu formato a faixa de {int} a {int} com o conector {string}',
  function (this: BusinessRulesWorld, min: number, max: number, separator: string) {
    this.result = formatSalaryRange(min, max, true, separator);
  }
);

When(
  'eu formato a faixa de {int} a {int} com o conector padrão',
  function (this: BusinessRulesWorld, min: number, max: number) {
    this.result = formatSalaryRange(min, max, true);
  }
);

When(
  'eu resumo o encerramento da vaga com status {string} e {int} posições',
  function (this: BusinessRulesWorld, status: string, positions: number) {
    this.result = summarizeJobClosure({ status: toStatus(status), positions, publishedAt: null, closedAt: null });
  }
);

Then(
  'o rótulo de status {string} com {int} posições deve ser {string}',
  function (this: BusinessRulesWorld, status: string, positions: number, expected: string) {
    expect(jobStatusLabel(toStatus(status), 'recruiter', positions)).to.equal(expected);
  }
);

Then('a faixa deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  // O Intl usa espaço não separável entre "R$" e o valor.
  expect(String(this.result).replace(/ /g, ' ')).to.equal(expected);
});

Then('a pílula deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect(summary(this).statusLabel).to.equal(expected);
});

Then('o título deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect(summary(this).title).to.equal(expected);
});

Then('o motivo deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect(summary(this).reason).to.equal(expected);
});

Then('a frase deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect(summary(this).headline).to.equal(expected);
});

Then('o período aberto deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect(summary(this).openPeriod).to.equal(expected);
});

Then('não deve haver período aberto', function (this: BusinessRulesWorld) {
  expect(summary(this).openPeriod).to.equal(null);
});
