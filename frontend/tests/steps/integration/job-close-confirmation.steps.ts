import {
  closeJobSuccessCopy,
  describeCloseJobConfirmation,
  jobStatusLabel,
  type OpenApplicationsCount
} from '@/features/recrutamento/vagas/close-job-copy';
import { closeJob, getOpenApplicationsCount } from '@/features/recrutamento/vagas/service/jobs-api';
import {
  closeJobResponseSchema,
  jobResponseSchema,
  openApplicationsCountResponseSchema
} from '@/features/recrutamento/vagas/service/jobs-response-schema';
import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { putCalls, recordedGets, recordedPuts, respondToGet, respondToPut } from '../../support/axios-double';
import type { BusinessRulesWorld } from '../../support/world';

/**
 * Fluxo da gestão da vaga: o estado decide se a acção existe, a confirmação diz o efeito
 * antes de executar e o feedback repete o efeito consumado que a API devolveu. A interacção
 * é roteirizada (o Cucumber deste projecto não renderiza React), mas o contrato de leitura,
 * a chamada HTTP e as redacções são as de produção. Só o transporte é duplo, partilhado em
 * `tests/support/axios-double.ts`.
 */

/** Estado da tela de gestão entre abrir a vaga, confirmar e receber o efeito. */
type CloseFlow = {
  jobId: number;
  isActive: boolean;
  /** O que a API tem para devolver — a tela só o conhece depois de ler a contagem. */
  openApplicationsOnServer: number;
  managementActions: string[];
  isConfirming: boolean;
  /** Estado da leitura da contagem, como a confirmação o vê. */
  count: OpenApplicationsCount;
  feedback: string | null;
};

function flow(world: BusinessRulesWorld): CloseFlow {
  return world.data.flow as CloseFlow;
}

/** Resposta completa de `JobViewModel`; cada cenário sobrescreve só o que lhe interessa. */
function completeJob(id: number, isActive: boolean): Record<string, unknown> {
  return {
    id,
    title: 'Operador(a) de Empilhadeira',
    summary: null,
    description: 'Movimentação de cargas no armazém.',
    companyId: 7,
    salaryMin: 2300,
    salaryMax: null,
    salaryDisclosed: true,
    jobType: 'Clt',
    workModel: 'OnSite',
    workShift: 'SegundoTurno',
    experienceLevel: 'AteUmAno',
    area: 'Logistica',
    isPcdFriendly: false,
    city: 'Extrema',
    state: 'MG',
    country: 'BR',
    requirements: [],
    benefits: [],
    isActive,
    publicationDate: '10/01/2026 09:00:00',
    publishedAt: '2026-01-10T12:00:00+00:00',
    createdAt: '10/01/2026 09:00:00',
    updatedAt: '',
    deletedAt: '',
    isDeleted: false
  };
}

function startFlow(this: BusinessRulesWorld, id: number, isActive: boolean, openApplicationsOnServer: number): void {
  const job = jobResponseSchema.parse(completeJob(id, isActive));

  // A contagem vive num endpoint próprio, autenticado e sem cache — não no detalhe da vaga.
  respondToGet(`/api/jobs/${job.id}/open-applications-count`, () => ({
    openApplicationsCount: openApplicationsOnServer
  }));

  // A API move as candidaturas em aberto para "Cancelada" e devolve quantas foram afectadas.
  respondToPut(`/api/jobs/${job.id}/close`, () => ({
    jobId: job.id,
    closedAt: '2026-09-01T20:11:04Z',
    affectedApplications: openApplicationsOnServer
  }));

  this.data.flow = {
    jobId: job.id,
    isActive: job.isActive,
    openApplicationsOnServer,
    managementActions: [],
    isConfirming: false,
    count: { status: 'counting' },
    feedback: null
  } satisfies CloseFlow;
}

Given(
  'que a vaga #{int} está activa com {int} candidaturas em aberto',
  function (this: BusinessRulesWorld, id: number, abertas: number) {
    startFlow.call(this, id, true, abertas);
  }
);

Given('que a vaga #{int} está encerrada', function (this: BusinessRulesWorld, id: number) {
  startFlow.call(this, id, false, 0);
});

When('eu abro a gestão da vaga', function (this: BusinessRulesWorld) {
  const current = flow(this);
  const actions = ['Ver candidatos', 'Salvar'];

  // V2/CA-18: a acção existe só em vaga activa — não se oferece o que a API vai recusar.
  if (current.isActive) actions.push('Encerrar vaga');

  current.managementActions = actions;
});

Given('que a leitura da contagem vai falhar', function () {
  // Sobrepõe-se à resposta declarada pelo `Dado` anterior para o mesmo endpoint.
  respondToGet('/open-applications-count', () => {
    throw new Error('Request failed with status code 500');
  });
});

/** Abrir a confirmação dispara a leitura da contagem — é o momento em que o número importa. */
When('eu escolho encerrar a vaga', async function (this: BusinessRulesWorld) {
  const current = flow(this);
  expect(current.isActive, 'a acção não deveria estar disponível numa vaga encerrada').to.equal(true);

  current.isConfirming = true;
  current.count = { status: 'counting' };

  try {
    current.count = { status: 'ready', count: await getOpenApplicationsCount(current.jobId) };
  } catch {
    current.count = { status: 'unavailable' };
  }
});

When('eu escolho encerrar a vaga sem esperar pela contagem', function (this: BusinessRulesWorld) {
  const current = flow(this);
  current.isConfirming = true;
  current.count = { status: 'counting' };
});

When('eu abandono a confirmação de encerramento', function (this: BusinessRulesWorld) {
  flow(this).isConfirming = false;
});

When('eu confirmo o encerramento', async function (this: BusinessRulesWorld) {
  const current = flow(this);
  expect(current.isConfirming, 'a confirmação precisa estar aberta antes de confirmar').to.equal(true);

  const result = await closeJob(current.jobId);
  current.isActive = false;
  current.feedback = closeJobSuccessCopy.describe(result.affectedApplications);
  current.isConfirming = false;
  this.result = result;
});

// A API antiga devolvia texto puro; com `responseType: 'json'`, o axios entrega-o como string.
When('eu leio a resposta de encerramento em texto {string}', function (this: BusinessRulesWorld, corpo: string) {
  this.data.closeParseResult = closeJobResponseSchema.safeParse(corpo);
});

When('eu leio uma resposta de encerramento sem {string}', function (this: BusinessRulesWorld, campo: string) {
  const payload: Record<string, unknown> = {
    jobId: 36,
    closedAt: '2026-09-01T20:11:04Z',
    affectedApplications: 3
  };
  delete payload[campo];

  this.data.closeParseResult = closeJobResponseSchema.safeParse(payload);
});

When('eu leio uma resposta de contagem sem {string}', function (this: BusinessRulesWorld, campo: string) {
  const payload: Record<string, unknown> = { openApplicationsCount: 3 };
  delete payload[campo];

  this.data.countParseResult = openApplicationsCountResponseSchema.safeParse(payload);
});

Then('o estado exibido na gestão da vaga deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(jobStatusLabel(flow(this).isActive)).to.equal(esperado);
});

Then('a acção {string} deve estar disponível na gestão', function (this: BusinessRulesWorld, rotulo: string) {
  const actions = flow(this).managementActions;
  expect(actions, `acções da gestão: ${actions.join(', ') || 'nenhuma'}`).to.include(rotulo);
});

Then('a acção {string} não deve estar disponível na gestão', function (this: BusinessRulesWorld, rotulo: string) {
  const actions = flow(this).managementActions;
  expect(actions, `acções da gestão: ${actions.join(', ') || 'nenhuma'}`).to.not.include(rotulo);
});

Then('a confirmação de encerramento deve estar aberta', function (this: BusinessRulesWorld) {
  expect(flow(this).isConfirming).to.equal(true);
});

Then('a confirmação de encerramento deve estar fechada', function (this: BusinessRulesWorld) {
  expect(flow(this).isConfirming).to.equal(false);
});

Then('a confirmação de encerramento deve dizer {string}', function (this: BusinessRulesWorld, esperado: string) {
  const texto = describeCloseJobConfirmation(flow(this).count);
  expect(texto, `texto da confirmação: ${texto}`).to.include(esperado);
});

Then('a confirmação de encerramento não deve dizer {string}', function (this: BusinessRulesWorld, proibido: string) {
  const texto = describeCloseJobConfirmation(flow(this).count);
  expect(texto, `texto da confirmação: ${texto}`).to.not.include(proibido);
});

Then(
  'a confirmação de encerramento deve avisar que a vaga não pode ser reativada',
  function (this: BusinessRulesWorld) {
    const texto = describeCloseJobConfirmation(flow(this).count);
    expect(texto, 'a confirmação precisa dizer que o encerramento não tem retorno').to.include(
      'não pode ser reativada'
    );
  }
);

/** Sem contagem o encerramento continua ao alcance: bloquear por leitura auxiliar seria pior. */
Then('o encerramento deve continuar possível', function (this: BusinessRulesWorld) {
  const current = flow(this);
  expect(current.isConfirming, 'a confirmação deveria continuar aberta').to.equal(true);
  expect(current.count.status, 'o cenário devia ter a contagem indisponível').to.equal('unavailable');
});

Then('a contagem de candidaturas em aberto não deve ter sido lida', function () {
  const leituras = recordedGets();
  expect(leituras, `leituras registradas: ${leituras.join(', ') || 'nenhuma'}`).to.have.length(0);
});

Then('a API deve ter lido {string}', function (esperado: string) {
  const leituras = recordedGets();
  expect(leituras, `leituras registradas: ${leituras.join(', ') || 'nenhuma'}`).to.include(esperado);
});

Then('a API não deve ter recebido o encerramento', function () {
  const chamadas = recordedPuts();
  expect(chamadas, `chamadas registradas: ${chamadas.join(', ') || 'nenhuma'}`).to.have.length(0);
});

Then('a API de vagas deve ter recebido {string}', function (esperado: string) {
  const chamadas = recordedPuts();
  expect(chamadas, `chamadas registradas: ${chamadas.join(', ') || 'nenhuma'}`).to.include(esperado);
});

Then('o pedido de encerramento não deve ter corpo', function () {
  expect(putCalls, 'nenhum pedido foi registrado').to.have.length.greaterThan(0);
  expect(putCalls[0].body, 'o endpoint de encerramento não recebe corpo').to.equal(undefined);
});

Then('o feedback de encerramento deve dizer {string}', function (this: BusinessRulesWorld, esperado: string) {
  const feedback = flow(this).feedback;
  expect(feedback, 'esperava o feedback do encerramento').to.not.equal(null);
  expect(feedback as string, `feedback: ${feedback}`).to.include(esperado);
});

Then('a leitura do encerramento deve falhar', function (this: BusinessRulesWorld) {
  const parsed = this.data.closeParseResult as ReturnType<typeof closeJobResponseSchema.safeParse>;
  expect(parsed.success, 'esperava que o contrato rejeitasse a resposta antiga').to.equal(false);
});

Then('a leitura do encerramento deve falhar no campo {string}', function (this: BusinessRulesWorld, campo: string) {
  const parsed = this.data.closeParseResult as ReturnType<typeof closeJobResponseSchema.safeParse>;
  expect(parsed.success, 'esperava que o contrato rejeitasse a resposta').to.equal(false);

  const paths = parsed.success ? [] : parsed.error.issues.map((issue) => issue.path.join('.'));
  expect(paths, `campos com erro: ${paths.join(', ')}`).to.include(campo);
});

Then('a leitura da contagem deve falhar no campo {string}', function (this: BusinessRulesWorld, campo: string) {
  const parsed = this.data.countParseResult as ReturnType<typeof openApplicationsCountResponseSchema.safeParse>;
  expect(parsed.success, 'esperava que o contrato rejeitasse a contagem incompleta').to.equal(false);

  const paths = parsed.success ? [] : parsed.error.issues.map((issue) => issue.path.join('.'));
  expect(paths, `campos com erro: ${paths.join(', ')}`).to.include(campo);
});
