import {
  applicationStatusLabel,
  canCandidateCancelApplication,
  parseApplicationStatus
} from '@/features/candidaturas/domain';
import { cancelApplicationDialogCopy } from '@/features/candidaturas/my/cancel-application-dialog-copy';
import { cancelJobApplication } from '@/features/candidaturas/service/job-applications-api';
import {
  jobApplicationResponseSchema,
  type JobApplicationResponse
} from '@/features/candidaturas/service/job-applications-response-schema';
import { axiosApi } from '@/shared/api';
import { After, Before, Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

/**
 * O fluxo real da tela é: a linha decide se oferece a ação, a confirmação segura o ato
 * (que é terminal) e só então o service fala com a API. Aqui a interação é roteirizada —
 * o Cucumber deste projeto não renderiza React —, mas a regra de disponibilidade, o texto
 * da confirmação e a chamada HTTP são os de produção, não cópias.
 */

type PutCall = { url: string; body: unknown };

const putCalls: PutCall[] = [];
let putFailure: Error | null = null;
let apiApplication: Record<string, unknown> = {};

/**
 * Duplo do transporte: `cancelJobApplication` e o contrato Zod continuam a correr de verdade.
 * Instalado no início de cada cenário e **restaurado no fim** — o `axiosApi` é um módulo único
 * para todo o processo do Cucumber, e um duplo que sobrevive ao cenário faria um teste futuro
 * passar contra esta resposta em vez da chamada real.
 */
const originalPut = axiosApi.put;

const putStub = (async (url: string, body: unknown) => {
  putCalls.push({ url, body });
  if (putFailure) throw putFailure;

  return {
    data: { ...apiApplication, status: 'CanceledByCandidate' },
    status: 200,
    statusText: 'OK',
    headers: {},
    config: {}
  };
}) as unknown as typeof axiosApi.put;

/** Estado que a tela mantém entre a escolha da ação e a confirmação. */
type CancelFlow = {
  application: JobApplicationResponse;
  rowActions: string[];
  pendingCancelId: number | null;
  canceled: JobApplicationResponse | null;
};

function flow(world: BusinessRulesWorld): CancelFlow {
  return world.data.flow as CancelFlow;
}

Before(function () {
  putCalls.length = 0;
  putFailure = null;
  apiApplication = {};
  axiosApi.put = putStub;
});

After(function () {
  axiosApi.put = originalPut;
});

Given(
  'que a minha candidatura #{int} está com o status {string}',
  function (this: BusinessRulesWorld, id: number, status: string) {
    apiApplication = {
      id,
      jobId: 3,
      candidate: { id: 5, name: 'ana.souza', email: 'ana.souza@empreganet.com.br', isDeleted: false },
      status,
      appliedAt: '10/01/2026 09:00:00',
      createdAt: '10/01/2026 09:00:00',
      updatedAt: '',
      deletedAt: '',
      isDeleted: false
    };

    this.data.flow = {
      application: jobApplicationResponseSchema.parse(apiApplication),
      rowActions: [],
      pendingCancelId: null,
      canceled: null
    } satisfies CancelFlow;
  }
);

Given(
  'que a API vai recusar o cancelamento com o código {int}',
  function (this: BusinessRulesWorld, statusCode: number) {
    putFailure = Object.assign(new Error(`Request failed with status code ${statusCode}`), {
      response: { status: statusCode, data: { code: 'INVALID_ACTION_FOR_STATUS' } }
    });
  }
);

When('eu abro as ações da candidatura', function (this: BusinessRulesWorld) {
  const { application } = flow(this);
  const actions: string[] = [];

  if (application.jobId) actions.push('Ver vaga');
  if (canCandidateCancelApplication(application.status)) actions.push('Cancelar candidatura');

  flow(this).rowActions = actions;
});

When('eu escolho cancelar a candidatura', function (this: BusinessRulesWorld) {
  const current = flow(this);
  expect(
    canCandidateCancelApplication(current.application.status),
    'a ação não deveria estar disponível neste status'
  ).to.equal(true);

  current.pendingCancelId = current.application.id;
});

When('eu abandono a confirmação', function (this: BusinessRulesWorld) {
  flow(this).pendingCancelId = null;
});

When('eu confirmo o cancelamento', async function (this: BusinessRulesWorld) {
  const current = flow(this);
  const pendingId = current.pendingCancelId;
  expect(pendingId, 'a confirmação precisa estar aberta antes de confirmar').to.not.equal(null);

  try {
    current.canceled = await cancelJobApplication(pendingId as number);
    current.application = current.canceled;
    this.error = undefined;
  } catch (err) {
    this.error = err;
  } finally {
    current.pendingCancelId = null;
  }
});

Then('a ação {string} deve estar disponível', function (this: BusinessRulesWorld, rotulo: string) {
  const actions = flow(this).rowActions;
  expect(actions, `ações da linha: ${actions.join(', ') || 'nenhuma'}`).to.include(rotulo);
});

Then('a ação {string} não deve estar disponível', function (this: BusinessRulesWorld, rotulo: string) {
  const actions = flow(this).rowActions;
  expect(actions, `ações da linha: ${actions.join(', ') || 'nenhuma'}`).to.not.include(rotulo);
});

Then('a confirmação de cancelamento deve estar aberta', function (this: BusinessRulesWorld) {
  expect(flow(this).pendingCancelId).to.equal(flow(this).application.id);
});

Then('a confirmação de cancelamento deve estar fechada', function (this: BusinessRulesWorld) {
  expect(flow(this).pendingCancelId).to.equal(null);
});

Then('a confirmação deve avisar que a ação não tem retorno', function (this: BusinessRulesWorld) {
  const texto = cancelApplicationDialogCopy.describe(flow(this).application.id);
  expect(texto, 'a confirmação precisa dizer que o cancelamento não tem retorno').to.include('não tem retorno');
});

Then('o botão que recusa a confirmação não deve se chamar {string}', function (rotulo: string) {
  expect(
    cancelApplicationDialogCopy.cancelLabel,
    'no contexto, "Cancelar" significaria cancelar a candidatura'
  ).to.not.equal(rotulo);
});

Then('a API deve ter recebido {string}', function (esperado: string) {
  const chamadas = putCalls.map((call) => `PUT ${call.url}`);
  expect(chamadas, `chamadas registradas: ${chamadas.join(', ') || 'nenhuma'}`).to.include(esperado);
});

Then('a API não deve ter sido chamada', function () {
  const chamadas = putCalls.map((call) => call.url);
  expect(chamadas, `chamadas registradas: ${chamadas.join(', ') || 'nenhuma'}`).to.have.length(0);
});

Then('o pedido de cancelamento não deve ter corpo', function () {
  expect(putCalls, 'nenhum pedido foi registrado').to.have.length.greaterThan(0);
  expect(putCalls[0].body, 'o endpoint de cancelamento não recebe corpo').to.equal(undefined);
});

Then('a candidatura devolvida deve estar com o status {string}', function (this: BusinessRulesWorld, status: string) {
  expect(flow(this).canceled?.status).to.equal(status);
});

Then('a candidatura deve continuar com o status {string}', function (this: BusinessRulesWorld, status: string) {
  expect(flow(this).application.status).to.equal(status);
});

Then('o rótulo do status na minha lista deve ser {string}', function (this: BusinessRulesWorld, rotulo: string) {
  const status = parseApplicationStatus(flow(this).application.status);
  expect(status, 'o status devolvido pela API não é reconhecido pelo contrato do frontend').to.not.equal(null);
  expect(applicationStatusLabel(status!, 'candidate')).to.equal(rotulo);
});

Then('o cancelamento deve ter falhado', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava que a recusa da API propagasse').to.not.equal(undefined);
});
