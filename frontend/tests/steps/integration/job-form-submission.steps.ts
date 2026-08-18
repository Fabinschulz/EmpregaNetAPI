import { Given, Then, When, type DataTable } from '@cucumber/cucumber';
import { expect } from 'chai';
import { jobRequestSchema } from '@/features/recrutamento/vagas/service/jobs-request-schema';
import {
  jobFormSchema,
  jobFormToRequest,
  jobFormValuesFromResponse
} from '@/features/recrutamento/vagas/form/job-form-schema';
import { jobResponseSchema } from '@/features/recrutamento/vagas/service/jobs-response-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

/**
 * Resposta completa de `JobViewModel`, usada como base dos cenários.
 *
 * Existe porque o `JobViewModel` não tem campo opcional entre os que o formulário consome:
 * enums são tipos-valor (nunca ausentes — no máximo `"NaoSelecionado"`), `Country`,
 * `Requirements`, `Benefits` e `PublicationDate` são `required`. Um docstring de cenário que
 * omitisse metade disso descreveria um payload que a API não sabe produzir, e o teste passaria
 * a validar uma ficção. Cada cenário sobrescreve só os campos sobre os quais fala.
 */
function completeJobResponse(): Record<string, unknown> {
  return {
    id: 1,
    title: 'Vaga completa',
    summary: null,
    description: 'Descrição da vaga.',
    companyId: 7,
    salaryMin: null,
    salaryMax: null,
    salaryDisclosed: true,
    jobType: 'Clt',
    workModel: 'OnSite',
    workShift: 'Administrativo',
    experienceLevel: 'SemExperiencia',
    area: 'Administrativo',
    isPcdFriendly: false,
    city: 'Extrema',
    state: 'MG',
    country: 'BR',
    requirements: [],
    benefits: [],
    isActive: true,
    publicationDate: '10/01/2026 09:00:00',
    publishedAt: '2026-01-10T12:00:00+00:00',
    createdAt: '10/01/2026 09:00:00',
    updatedAt: '',
    deletedAt: '',
    isDeleted: false
  };
}

Given('que a API devolveu os dados desta vaga cadastrada:', function (this: BusinessRulesWorld, rawJson: string) {
  this.data.rawJob = { ...completeJobResponse(), ...JSON.parse(rawJson) };
});

Given(
  'que a API devolveu esta resposta de vaga, exatamente como está:',
  function (this: BusinessRulesWorld, rawJson: string) {
    this.data.rawJob = JSON.parse(rawJson);
  }
);

When('eu carrego esses dados no formulário de edição de vaga', function (this: BusinessRulesWorld) {
  const dto = jobResponseSchema.parse(this.data.rawJob);
  this.data.jobFormValues = jobFormValuesFromResponse(dto);
});

When(
  'eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada',
  function (this: BusinessRulesWorld) {
    const formValues = jobFormSchema.parse(this.data.jobFormValues);
    this.result = jobFormToRequest(formValues);
  }
);

Then('o payload de reenvio da vaga deve satisfazer o contrato de requisição', function (this: BusinessRulesWorld) {
  const parsed = jobRequestSchema.safeParse(this.result);
  const issues = parsed.success ? '' : parsed.error.issues.map((i) => `${i.path.join('.')}: ${i.message}`).join(' | ');
  expect(parsed.success, `o mapeamento produziu um corpo que a API recusaria — ${issues}`).to.equal(true);
});

Then('o payload de reenvio da vaga deve conter:', function (this: BusinessRulesWorld, table: DataTable) {
  const payload = this.result as Record<string, unknown>;
  for (const { campo, valor } of table.hashes()) {
    const actual = getByPath(payload, campo);
    expect(String(actual), `campo "${campo}"`).to.equal(valor);
  }
});

Then(
  'o campo {string} do payload de reenvio deve ser do tipo número',
  function (this: BusinessRulesWorld, campo: string) {
    const payload = this.result as Record<string, unknown>;
    expect(getByPath(payload, campo), `campo "${campo}"`).to.be.a('number');
  }
);

Then('o campo {string} do payload de reenvio deve estar ausente', function (this: BusinessRulesWorld, campo: string) {
  const payload = this.result as Record<string, unknown>;
  expect(getByPath(payload, campo), `campo "${campo}"`).to.equal(undefined);
});

When('eu valido os dados carregados no formulário de vaga', function (this: BusinessRulesWorld) {
  this.result = jobFormSchema.safeParse(this.data.jobFormValues).success;
});

Then('os dados carregados devem ser rejeitados pelo formulário', function (this: BusinessRulesWorld) {
  expect(this.result, 'esperava que o formulário rejeitasse a vaga legada incompleta').to.equal(false);
});

When('eu valido esses dados contra o contrato de leitura de vaga', function (this: BusinessRulesWorld) {
  this.data.jobParseResult = jobResponseSchema.safeParse(this.data.rawJob);
});

Then(
  'a validação do contrato de vaga deve falhar no campo {string}',
  function (this: BusinessRulesWorld, campo: string) {
    const parsed = this.data.jobParseResult as ReturnType<typeof jobResponseSchema.safeParse>;
    expect(parsed.success, 'esperava que o contrato rejeitasse a resposta').to.equal(false);

    const paths = parsed.success ? [] : parsed.error.issues.map((issue) => issue.path.join('.'));
    expect(paths, `campos com erro: ${paths.join(', ')}`).to.include(campo);
  }
);

Then('o rótulo de situação da vaga deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  const dto = jobResponseSchema.parse(this.data.rawJob);
  // Mesma expressão usada na listagem de recrutamento e no detalhe público.
  expect(dto.isActive ? 'Ativa' : 'Encerrada').to.equal(esperado);
});
