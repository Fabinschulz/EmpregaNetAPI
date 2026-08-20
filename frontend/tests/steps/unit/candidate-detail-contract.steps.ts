import { Then, When } from '@cucumber/cucumber';
// Ficheiro do schema em vez do barrel do service: o barrel exporta hooks de client component.
import {
  candidateDetailResponseSchema,
  candidateDisplayName,
  type CandidateDetailResponse
} from '@/features/recrutamento/candidatos/service/candidate-detail-response-schema';
import { expect } from 'chai';
import { getByPath } from '../../support/object-path';
import type { BusinessRulesWorld } from '../../support/world';

/** Payload íntegro devolvido pela API; cada cenário degrada um campo a partir dele. */
function validCandidatePayload(): Record<string, unknown> {
  return {
    id: 5,
    username: 'QA_E2E_Tester',
    email: 'qa.e2e.tester@example.com',
    phoneNumber: '11988887777',
    userType: 'Candidato',
    roles: ['Candidate'],
    profilePicture: null,
    city: 'Extrema',
    state: 'MG',
    age: 34,
    createdAt: '2026-01-10T12:00:00+00:00',
    updatedAt: '2026-02-01T09:30:00+00:00',
    isDeleted: false,
    applications: {
      total: 3,
      byStatus: [
        { status: 'Processing', count: 2 },
        { status: 'Approved', count: 1 }
      ]
    }
  };
}

function readCandidate(this: BusinessRulesWorld, payload: unknown): void {
  const parsed = candidateDetailResponseSchema.safeParse(payload);
  this.result = parsed.success ? parsed.data : undefined;
  this.error = parsed.success ? undefined : parsed.error;
}

function parsedCandidate(world: BusinessRulesWorld): CandidateDetailResponse {
  return world.result as CandidateDetailResponse;
}

When('eu leio a ficha do candidato', function (this: BusinessRulesWorld) {
  readCandidate.call(this, validCandidatePayload());
});

When('eu leio a ficha do candidato com {string} nulo', function (this: BusinessRulesWorld, campo: string) {
  readCandidate.call(this, { ...validCandidatePayload(), [campo]: null });
});

When('eu leio a ficha do candidato sem {string}', function (this: BusinessRulesWorld, campo: string) {
  const payload = validCandidatePayload();
  delete payload[campo];
  readCandidate.call(this, payload);
});

When(
  'eu leio a ficha do candidato com {string} e {string} vazios',
  function (this: BusinessRulesWorld, primeiro: string, segundo: string) {
    readCandidate.call(this, { ...validCandidatePayload(), [primeiro]: '', [segundo]: '' });
  }
);

When(
  'eu leio a ficha do candidato com {string} igual a {string}',
  function (this: BusinessRulesWorld, campo: string, valor: string) {
    readCandidate.call(this, { ...validCandidatePayload(), [campo]: valor });
  }
);

When('eu leio a ficha do candidato com o resumo de candidaturas renomeado', function (this: BusinessRulesWorld) {
  const payload = validCandidatePayload();
  payload.applicationsSummary = payload.applications;
  delete payload.applications;
  readCandidate.call(this, payload);
});

Then(
  'a ficha lida deve ter {string} igual a {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    expect(getByPath(parsedCandidate(this) as unknown as Record<string, unknown>, campo)).to.equal(esperado);
  }
);

Then('a ficha lida deve ter {string} nulo', function (this: BusinessRulesWorld, campo: string) {
  expect(getByPath(parsedCandidate(this) as unknown as Record<string, unknown>, campo)).to.equal(null);
});

Then('a ficha lida deve ter {int} candidaturas no total', function (this: BusinessRulesWorld, total: number) {
  expect(parsedCandidate(this).applications.total).to.equal(total);
});

Then('o nome exibível do candidato deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(candidateDisplayName(parsedCandidate(this))).to.equal(esperado);
});
