import {
  APPLICATION_STATUSES,
  applicationStatusLabel,
  applicationStatusTransitions,
  canCandidateCancelApplication,
  parseApplicationStatus,
  type ApplicationStatus,
  type ApplicationStatusAudience
} from '@/features/candidaturas/domain';
import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

const AUDIENCES: readonly ApplicationStatusAudience[] = ['candidate', 'recruiter'];

function audienceByName(name: string): ApplicationStatusAudience {
  const audience = AUDIENCES.find((candidate) => candidate === name);
  expect(audience, `visão desconhecida: ${name}`).to.not.equal(undefined);
  return audience as ApplicationStatusAudience;
}

/** O Gherkin só carrega texto: o status vem cru, como a API o devolveria. */
function knownStatus(raw: string): ApplicationStatus {
  const parsed = parseApplicationStatus(raw);
  expect(parsed, `status desconhecido no cenário: ${raw}`).to.not.equal(null);
  return parsed as ApplicationStatus;
}

When('eu inspeciono o vocabulário de status de candidatura', function (this: BusinessRulesWorld) {
  this.data.statuses = APPLICATION_STATUSES;
});

When('eu interpreto o status {string}', function (this: BusinessRulesWorld, status: string) {
  this.result = parseApplicationStatus(status);
});

When(
  'eu leio o rótulo do status {string} na visão {string}',
  function (this: BusinessRulesWorld, status: string, visao: string) {
    this.result = applicationStatusLabel(knownStatus(status), audienceByName(visao));
  }
);

When(
  'eu verifico se o candidato pode cancelar uma candidatura {string}',
  function (this: BusinessRulesWorld, status: string) {
    this.result = canCandidateCancelApplication(status);
  }
);

Then('todos os status devem ter rótulo em português nas duas visões', function () {
  // Identificador de enum não traduzido é reconhecível por ser PascalCase colado.
  const looksLikeEnumIdentifier = /[a-z][A-Z]/;

  AUDIENCES.forEach((audience) => {
    APPLICATION_STATUSES.forEach((status) => {
      const label = applicationStatusLabel(status, audience);
      expect(label.trim(), `o status "${status}" está sem rótulo na visão "${audience}"`).to.not.equal('');
      expect(
        looksLikeEnumIdentifier.test(label),
        `o rótulo de "${status}" na visão "${audience}" parece o identificador do enum`
      ).to.equal(false);
    });
  });
});

Then('nenhum rótulo de status deve repetir dentro da mesma visão', function () {
  AUDIENCES.forEach((audience) => {
    const labels = APPLICATION_STATUSES.map((status) => applicationStatusLabel(status, audience));
    expect(new Set(labels).size, `rótulos repetidos na visão "${audience}": ${labels.join(', ')}`).to.equal(
      labels.length
    );
  });
});

Then('o status interpretado deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result ?? '').to.equal(esperado);
});

Then('o rótulo do status deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});

Then('o cancelamento pelo candidato deve estar {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(['disponível', 'indisponível'], `disponibilidade inválida: ${esperado}`).to.include(esperado);
  expect(this.result).to.equal(esperado === 'disponível');
});

Then('o status {string} não deve ter transição de saída', function (status: string) {
  expect(applicationStatusTransitions[knownStatus(status)]).to.have.length(0);
});

Then('nenhuma transição do recrutador deve levar a {string}', function (status: string) {
  const target = knownStatus(status);
  const origins = APPLICATION_STATUSES.filter((origin) => applicationStatusTransitions[origin].includes(target));

  expect(origins, `o recrutador não cancela em nome do candidato; origens: ${origins.join(', ')}`).to.have.length(0);
});
