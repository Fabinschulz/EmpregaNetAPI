import { type DomainErrorResponse } from '@/shared/schema';
import { isAxiosError } from 'axios';
import { ApiStatusCode, isServerError } from '../enums';
import { isApiContractError, isContractViolation, type ContractIssue } from './api-contract-error';
import { breakdownDomainError, tryParseDomainError, UNKNOWN_ERROR_MESSAGE, type FieldErrorMap } from './domain-error';

/**
 * Natureza da falha, para quem precisa de reagir de forma diferente a cada uma.
 *
 * - `network`: o pedido não obteve resposta — conexão em baixo ou tempo esgotado.
 * - `validation`: a API recusou os dados enviados (400/422) e normalmente indica os campos.
 * - `unauthorized`: sessão ausente ou expirada (401).
 * - `forbidden`: sessão válida sem permissão para a operação (403).
 * - `notFound`: o recurso pedido não existe (404).
 * - `conflict`: choque com o estado atual do recurso (409).
 * - `business`: 4xx com código de domínio — regra de negócio recusada, não defeito técnico.
 * - `server`: a API falhou a processar (5xx). Pode ser transitório.
 * - `contract`: a resposta não encaixa no schema esperado. Defeito nosso, não do utilizador.
 * - `unknown`: nem sequer é um erro de rede reconhecível.
 */
export type ApiErrorKind =
  | 'network'
  | 'validation'
  | 'unauthorized'
  | 'forbidden'
  | 'notFound'
  | 'conflict'
  | 'business'
  | 'server'
  | 'contract'
  | 'unknown';

export type ApiErrorResult = {
  /** Natureza da falha. */
  kind: ApiErrorKind;
  message: string;
  statusCode?: number;
  domainError: DomainErrorResponse | null;
  /** Liga o relato do utilizador ao log do backend. A API devolve no corpo ou no header. */
  correlationId?: string;
  /** Falhas atribuídas a um campo do formulário. Vazio quando não há nenhuma. */
  fieldErrors: FieldErrorMap;
  /** Endpoint que quebrou o contrato (só em `kind: 'contract'` vindo de `ApiContractError`). */
  endpoint?: string;
  /** Divergências concretas entre o schema e o corpo recebido (só em `kind: 'contract'`). */
  contractIssues?: readonly ContractIssue[];
};

const NO_FIELD_ERRORS: FieldErrorMap = {};

const CONTRACT_ERROR_MESSAGE =
  'A resposta do servidor não corresponde ao formato esperado. Se o problema persistir, contacte o suporte.';
const NETWORK_ERROR_MESSAGE = 'Não foi possível conectar ao servidor. Verifique a sua conexão e tente novamente.';
const TIMEOUT_ERROR_MESSAGE = 'A solicitação demorou mais que o esperado e foi interrompida. Tente novamente.';

/** Códigos com que o axios sinaliza tempo esgotado, por oposição a conexão recusada. */
const TIMEOUT_CODES: readonly string[] = ['ECONNABORTED', 'ETIMEDOUT'];

const CORRELATION_ID_HEADER = 'x-correlation-id';

function mapStatusToErrorKind(status: number, domainError: DomainErrorResponse | null): ApiErrorKind {
  switch (status) {
    case ApiStatusCode.Unauthorized:
      return 'unauthorized';
    case ApiStatusCode.Forbidden:
      return 'forbidden';
    case ApiStatusCode.NotFound:
      return 'notFound';
    case ApiStatusCode.Conflict:
      return 'conflict';
    case ApiStatusCode.BadRequest:
    case ApiStatusCode.UnprocessableEntity:
      return 'validation';
  }

  if (isServerError(status)) return 'server';

  // 4xx restante com código de domínio é regra de negócio recusada, não erro técnico.
  if (status >= ApiStatusCode.BadRequest && domainError?.code !== undefined) return 'business';

  return 'unknown';
}

function defaultHttpStatusMessage(status: number, resource?: string): string {
  const target = resource ?? 'o recurso';

  switch (status) {
    case ApiStatusCode.BadRequest:
      return `Pedido inválido ao acessar ${target}.`;
    case ApiStatusCode.Unauthorized:
      return 'Acesso não autorizado. Por favor, inicie sessão novamente.';
    case ApiStatusCode.Forbidden:
      return 'Não tem permissão para acessar este recurso.';
    case ApiStatusCode.NotFound:
      return `Não encontramos ${target}.`;
    case ApiStatusCode.Conflict:
      return 'Este registo conflita com um já existente.';
    case ApiStatusCode.UnprocessableEntity:
      return 'Os dados enviados são inválidos. Reveja o formulário e tente novamente.';
    case ApiStatusCode.TooManyRequests:
      return 'Muitas tentativas em pouco tempo. Aguarde alguns instantes.';
    default:
      if (isServerError(status)) return 'Erro interno do servidor. Tente novamente mais tarde.';
      return `Não foi possível comunicar com o servidor ao acessar ${target}.`;
  }
}

/** O `correlationId` vem no corpo `DomainError`; sem corpo, o header ainda o traz. */
function readCorrelationId(domainError: DomainErrorResponse | null, headers: unknown): string | undefined {
  if (domainError?.correlationId) return domainError.correlationId;

  if (headers === null || typeof headers !== 'object') return undefined;

  const value = (headers as Record<string, unknown>)[CORRELATION_ID_HEADER];
  return typeof value === 'string' && value.trim().length > 0 ? value.trim() : undefined;
}

/**
 * Classifica qualquer falha vinda da camada de API numa forma única, pronta para a UI.
 *
 * <para>`resource` é o substantivo do recurso (ex.: `'vaga'`), usado nas mensagens por status.</para>
 */
export function parseApiError(err: unknown, resource?: string): ApiErrorResult {
  if (isContractViolation(err)) {
    return {
      kind: 'contract',
      message: CONTRACT_ERROR_MESSAGE,
      domainError: null,
      fieldErrors: NO_FIELD_ERRORS,
      endpoint: isApiContractError(err) ? err.endpoint : undefined,
      contractIssues: isApiContractError(err) ? err.issues : undefined
    };
  }

  if (!isAxiosError(err)) {
    const message = err instanceof Error && err.message.trim().length > 0 ? err.message.trim() : UNKNOWN_ERROR_MESSAGE;
    return { kind: 'unknown', message, domainError: null, fieldErrors: NO_FIELD_ERRORS };
  }

  // Sem `response` o pedido nunca chegou a ter resposta: distinguir tempo esgotado de rede em
  // baixo muda o que o utilizador deve fazer a seguir (repetir vs. verificar a ligação).
  if (!err.response) {
    const isTimeout = err.code !== undefined && TIMEOUT_CODES.includes(err.code);
    return {
      kind: 'network',
      message: isTimeout ? TIMEOUT_ERROR_MESSAGE : NETWORK_ERROR_MESSAGE,
      domainError: null,
      fieldErrors: NO_FIELD_ERRORS
    };
  }

  const status = err.response.status;
  const domainError = tryParseDomainError(err.response.data);
  const correlationId = readCorrelationId(domainError, err.response.headers);

  if (domainError) {
    const { message, fieldErrors } = breakdownDomainError(domainError, resource);
    const statusCode = domainError.statusCode ?? status;

    return {
      // Campos identificados pelo servidor são sempre erro de validação, independente do status.
      kind: Object.keys(fieldErrors).length > 0 ? 'validation' : mapStatusToErrorKind(statusCode, domainError),
      message,
      statusCode,
      domainError,
      correlationId,
      fieldErrors
    };
  }

  return {
    kind: mapStatusToErrorKind(status, null),
    message: defaultHttpStatusMessage(status, resource),
    statusCode: status,
    domainError: null,
    correlationId,
    fieldErrors: NO_FIELD_ERRORS
  };
}
