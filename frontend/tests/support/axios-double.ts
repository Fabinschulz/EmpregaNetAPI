import { axiosApi } from '@/shared/api';

/**
 * Duplo **único** do transporte HTTP (`axiosApi.put` e `axiosApi.get`) nos cenários de integração.
 *
 * Existe porque `axiosApi` é um módulo único em todo o processo do Cucumber: enquanto cada
 * ficheiro de steps instalava o seu próprio duplo num `Before`, o último a ser registado
 * ganhava, e o cenário do outro ficheiro recebia a resposta errada — o teste falhava por
 * contrato quando o código estava correcto. Com um duplo só, cada cenário declara o que
 * responder **por endpoint** (`respondToPut`/`respondToGet`), e a instalação/restauro fica em `hooks.ts`.
 */

export type PutCall = { url: string; body: unknown };

/** Chamadas registadas no cenário actual, na ordem em que saíram. */
export const putCalls: PutCall[] = [];

/** Devolve o corpo da resposta, ou lança para simular a recusa da API. */
export type PutResponder = (call: PutCall) => unknown;

type PutStub = { pattern: string | RegExp; respond: PutResponder };

const stubs: PutStub[] = [];
const originalPut = axiosApi.put;

function matches(pattern: string | RegExp, url: string): boolean {
  return typeof pattern === 'string' ? url.includes(pattern) : pattern.test(url);
}

const putDouble = (async (url: string, body: unknown) => {
  const call: PutCall = { url, body };
  putCalls.push(call);

  // Do mais recente para o mais antigo: endpoints distintos coexistem (um cenário pode
  // declarar vários), e uma redeclaração do mesmo endpoint sobrepõe-se à anterior — que é
  // como um "Dado ... vai recusar" refina o estado montado pelo "Dado" anterior.
  const stub = [...stubs].reverse().find((candidate) => matches(candidate.pattern, url));

  if (!stub) {
    const declarados = stubs.map((candidate) => String(candidate.pattern)).join(', ') || 'nenhum';
    throw new Error(`Nenhuma resposta declarada para PUT ${url}. Endpoints declarados: ${declarados}.`);
  }

  return { data: stub.respond(call), status: 200, statusText: 'OK', headers: {}, config: {} };
}) as unknown as typeof axiosApi.put;

/**
 * Declara a resposta de um endpoint no cenário actual.
 *
 * @param pattern trecho da URL (ou expressão) que identifica o endpoint — não um curinga global,
 *   para que declarar o encerramento de vaga não intercepte o cancelamento de candidatura.
 */
export function respondToPut(pattern: string | RegExp, respond: PutResponder): void {
  stubs.push({ pattern, respond });
}
/** URLs já chamadas, prefixadas com o método, para mensagens de asserção legíveis. */

export function recordedPuts(): string[] {
  return putCalls.map((call) => `PUT ${call.url}`);
}

/* --------------------------------------------------------------------------------------------
 * GET — mesma disciplina do PUT.
 * A confirmação de encerramento lê a contagem de candidaturas em aberto no momento em que abre,
 * por um endpoint próprio; sem um duplo de GET, o cenário não conseguiria distinguir "não leu"
 * de "leu e ignorou".
 * ------------------------------------------------------------------------------------------ */

export type GetCall = { url: string };

export const getCalls: GetCall[] = [];

export type GetResponder = (call: GetCall) => unknown;

type GetStub = { pattern: string | RegExp; respond: GetResponder };

const getStubs: GetStub[] = [];
const originalGet = axiosApi.get;

const getDouble = (async (url: string) => {
  const call: GetCall = { url };
  getCalls.push(call);

  const stub = [...getStubs].reverse().find((candidate) => matches(candidate.pattern, url));

  if (!stub) {
    const declarados = getStubs.map((candidate) => String(candidate.pattern)).join(', ') || 'nenhum';
    throw new Error(`Nenhuma resposta declarada para GET ${url}. Endpoints declarados: ${declarados}.`);
  }

  return { data: stub.respond(call), status: 200, statusText: 'OK', headers: {}, config: {} };
}) as unknown as typeof axiosApi.get;

export function respondToGet(pattern: string | RegExp, respond: GetResponder): void {
  getStubs.push({ pattern, respond });
}

export function installHttpDoubles(): void {
  putCalls.length = 0;
  stubs.length = 0;
  getCalls.length = 0;
  getStubs.length = 0;
  axiosApi.put = putDouble;
  axiosApi.get = getDouble;
}

export function restoreHttp(): void {
  stubs.length = 0;
  getStubs.length = 0;
  axiosApi.put = originalPut;
  axiosApi.get = originalGet;
}

/** URLs já lidas, prefixadas com o método, para mensagens de asserção legíveis. */
export function recordedGets(): string[] {
  return getCalls.map((call) => `GET ${call.url}`);
}
