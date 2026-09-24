import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  createUrlSyncState,
  lastKnownQuery,
  reconcileQuery,
  registerWrite,
  type UrlSyncState
} from '@/shared/hooks/url-sync-state';
import type { BusinessRulesWorld } from '../../support/world';

/**
 * Os valores do filtro são opacos para a máquina de estados; aqui cada um é um rótulo que diz de onde
 * veio (lido no mount, relido da URL ou escrito pelo formulário), para o cenário afirmar a origem.
 */
type Values = string;

const mountedFrom = (query: string): Values => `montado:${query}`;
const rereadFrom = (query: string): Values => `relido:${query}`;
const writtenFor = (query: string): Values => `escrito:${query}`;

type UrlSyncWorldData = {
  state?: UrlSyncState<Values>;
  /** Estado imediatamente antes da última conciliação, para checar a identidade do objeto. */
  stateBeforeReconcile?: UrlSyncState<Values>;
  external?: boolean;
  shouldReplace?: boolean;
  rereadCount: number;
};

function syncData(world: BusinessRulesWorld): UrlSyncWorldData {
  const data = world.data as Partial<UrlSyncWorldData>;
  data.rereadCount ??= 0;
  return data as UrlSyncWorldData;
}

function currentState(world: BusinessRulesWorld): UrlSyncState<Values> {
  const { state } = syncData(world);
  expect(state, 'a tela ainda não foi aberta no cenário').to.not.equal(undefined);
  return state!;
}

/**
 * Lista de queries do Gherkin separada por `|` (queries já usam `&` e `,`). Texto vazio é lista vazia;
 * um item vazio (`"status=Approved | "`) é a query limpa `""`.
 */
function parseQueryList(raw: string): string[] {
  return raw.trim() === '' ? [] : raw.split('|').map((item) => item.trim());
}

function write(world: BusinessRulesWorld, query: string, remount: boolean): void {
  const data = syncData(world);
  const { state, shouldReplace } = registerWrite(currentState(world), writtenFor(query), query, remount);
  data.state = state;
  data.shouldReplace = shouldReplace;
}

Given('que a tela abriu com a URL {string}', function (this: BusinessRulesWorld, query: string) {
  syncData(this).state = createUrlSyncState(mountedFrom(query), query);
});

Given('o formulário escreveu antes as queries {string}', function (this: BusinessRulesWorld, queries: string) {
  parseQueryList(queries).forEach((query) => write(this, query, false));
});

When('o formulário escreve a query {string}', function (this: BusinessRulesWorld, query: string) {
  write(this, query, false);
});

When('o reset escreve a query {string}', function (this: BusinessRulesWorld, query: string) {
  write(this, query, true);
});

/** O que o hook faz a cada render: concilia o estado com a query atual do router. */
When('o router devolve a query {string}', function (this: BusinessRulesWorld, query: string) {
  const data = syncData(this);
  const before = currentState(this);
  const result = reconcileQuery(before, query, () => {
    data.rereadCount += 1;
    return rereadFrom(query);
  });

  data.stateBeforeReconcile = before;
  data.state = result.state;
  data.external = result.external;
});

Then('a query deve ser tratada como mudança externa', function (this: BusinessRulesWorld) {
  expect(syncData(this).external).to.equal(true);
});

Then('a query não deve ser tratada como mudança externa', function (this: BusinessRulesWorld) {
  expect(syncData(this).external).to.equal(false);
});

Then('o estado deve ser o mesmo objeto de antes', function (this: BusinessRulesWorld) {
  const { stateBeforeReconcile } = syncData(this);
  expect(stateBeforeReconcile, 'nenhuma conciliação aconteceu no cenário').to.not.equal(undefined);
  expect(currentState(this)).to.equal(stateBeforeReconcile);
});

Then('os valores devem ter sido relidos da URL {string}', function (this: BusinessRulesWorld, query: string) {
  expect(syncData(this).rereadCount, 'readValues não foi chamado').to.be.greaterThan(0);
  expect(currentState(this).values).to.equal(rereadFrom(query));
});

Then('os valores não devem ter sido relidos da URL', function (this: BusinessRulesWorld) {
  expect(syncData(this).rereadCount).to.equal(0);
});

Then('os valores do filtro devem ser os escritos para {string}', function (this: BusinessRulesWorld, query: string) {
  expect(currentState(this).values).to.equal(writtenFor(query));
});

Then('o formulário deve ter remontado {int} vezes', function (this: BusinessRulesWorld, total: number) {
  expect(currentState(this).resetKey).to.equal(total);
});

Then('não deve haver escrita pendente', function (this: BusinessRulesWorld) {
  expect(currentState(this).pendingWrites).to.deep.equal([]);
});

Then('as escritas pendentes devem ser {string}', function (this: BusinessRulesWorld, queries: string) {
  expect([...currentState(this).pendingWrites]).to.deep.equal(parseQueryList(queries));
});

Then('a query conhecida deve ser {string}', function (this: BusinessRulesWorld, query: string) {
  expect(lastKnownQuery(currentState(this))).to.equal(query);
});

Then('o router.replace deve ser chamado', function (this: BusinessRulesWorld) {
  expect(syncData(this).shouldReplace).to.equal(true);
});

Then('o router.replace não deve ser chamado', function (this: BusinessRulesWorld) {
  expect(syncData(this).shouldReplace).to.equal(false);
});
