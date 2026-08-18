import {
  candidateDisplayName,
  jobApplicationResponseSchema,
  jobApplicationsListResponseSchema,
  type JobApplicationResponse
} from '@/features/candidaturas/service';
import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

/** Resposta completa de `JobApplicationViewModel`; cada cenário degrada um campo a partir dela. */
function completeApplicationResponse(): Record<string, unknown> {
  return {
    id: 12,
    jobId: 3,
    candidate: { id: 5, name: 'ana.souza', email: 'ana.souza@empreganet.com.br', isDeleted: false },
    status: 'Processing',
    appliedAt: '10/01/2026 09:00:00',
    createdAt: '10/01/2026 09:00:00',
    updatedAt: '',
    deletedAt: '',
    isDeleted: false
  };
}

function readApplication(this: BusinessRulesWorld, payload: unknown): void {
  const parsed = jobApplicationResponseSchema.safeParse(payload);
  this.result = parsed.success ? parsed.data : undefined;
  this.error = parsed.success ? undefined : parsed.error;
}

function parsedApplication(world: BusinessRulesWorld): JobApplicationResponse {
  return world.result as JobApplicationResponse;
}

When('eu leio a resposta de candidatura padrão', function (this: BusinessRulesWorld) {
  readApplication.call(this, completeApplicationResponse());
});

When('eu leio uma resposta de candidatura sem {string}', function (this: BusinessRulesWorld, campo: string) {
  const payload = completeApplicationResponse();
  delete payload[campo];
  readApplication.call(this, payload);
});

When(
  'eu leio uma resposta de candidatura no formato antigo, com {string}',
  function (this: BusinessRulesWorld, campo: string) {
    const payload = completeApplicationResponse();
    delete payload.candidate;
    payload[campo] = 5;
    readApplication.call(this, payload);
  }
);

When('eu leio uma resposta de candidatura com o nome do candidato vazio', function (this: BusinessRulesWorld) {
  const payload = completeApplicationResponse();
  readApplication.call(this, { ...payload, candidate: { id: 5, name: '', email: '', isDeleted: false } });
});

When('eu leio uma resposta de candidatura de um candidato com a conta encerrada', function (this: BusinessRulesWorld) {
  const payload = completeApplicationResponse();
  readApplication.call(this, {
    ...payload,
    candidate: { id: 5, name: 'ana.souza', email: 'ana.souza@empreganet.com.br', isDeleted: true }
  });
});

When(
  'eu leio uma listagem de candidaturas em que a segunda não tem nome de candidato',
  function (this: BusinessRulesWorld) {
    const base = completeApplicationResponse();
    const parsed = jobApplicationsListResponseSchema.safeParse({
      page: 1,
      totalPages: 1,
      totalItems: 3,
      data: [
        base,
        { ...base, id: 13, candidate: { id: 6, name: '', email: '', isDeleted: false } },
        { ...base, id: 14 }
      ]
    });

    this.result = parsed.success ? parsed.data : undefined;
    this.error = parsed.success ? undefined : parsed.error;
  }
);

Then('a leitura da candidatura deve ter sucesso', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava um parse bem-sucedido').to.equal(undefined);
});

Then('a leitura da candidatura deve falhar no campo {string}', function (this: BusinessRulesWorld, campo: string) {
  expect(this.error, 'esperava um erro de contrato').to.not.equal(undefined);

  const issues = (this.error as { issues: { path: (string | number | symbol)[] }[] }).issues;
  const paths = issues.map((issue) => issue.path.join('.'));
  expect(paths, `campos com erro: ${paths.join(', ')}`).to.include(campo);
});

Then('o candidato lido deve se chamar {string}', function (this: BusinessRulesWorld, nome: string) {
  expect(parsedApplication(this).candidate.name).to.equal(nome);
});

Then('o candidato lido deve estar marcado como excluído', function (this: BusinessRulesWorld) {
  expect(parsedApplication(this).candidate.isDeleted).to.equal(true);
});

Then('o rótulo do candidato na célula deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(candidateDisplayName(parsedApplication(this).candidate)).to.equal(esperado);
});

Then('a listagem de candidaturas lida deve conter {int} itens', function (this: BusinessRulesWorld, total: number) {
  const list = this.result as { data: JobApplicationResponse[] };
  expect(list.data).to.have.length(total);
});
