import { type DomainErrorResponse } from '@/shared/schema';
import { isAxiosError } from 'axios';
import { ApiStatusCode } from '../enums';
import { isContractViolation } from '../errors/api-contract-error';
import { breakdownDomainError, tryParseDomainError, type FieldErrorMap } from '../errors/domain-error';

/**
 * Natureza da falha, para quem precisa de reagir de forma diferente a cada uma.
 *
 * - `domain`: a API recusou o pedido e explicou porquê (corpo `DomainError`).
 * - `http`: respondeu com um status de erro sem corpo interpretável.
 * - `contract`: respondeu com sucesso, mas o corpo não encaixa no schema. Defeito nosso.
 * - `unknown`: não é sequer um erro de rede.
 */
export type ApiErrorKind = 'domain' | 'http' | 'contract' | 'unknown';

export type ApiErrorResult = {
  kind: ApiErrorKind;
  message: string;
  statusCode?: number;
  domainError: DomainErrorResponse | null;
  correlationId?: string;
  fieldErrors: FieldErrorMap;
};

const NO_FIELD_ERRORS: FieldErrorMap = {};

const UNKNOWN_ERROR_MESSAGE = 'Erro desconhecido. Por favor, entre em contato com o suporte técnico.';
const CONTRACT_ERROR_MESSAGE =
  'A resposta do servidor não corresponde ao formato esperado. Se o problema persistir, contacte o suporte.';

function defaultHttpStatusMessage(status: number | undefined, resource?: string): string {
  const target = resource ?? 'o recurso';

  switch (status) {
    case ApiStatusCode.Unauthorized:
      return 'Acesso não autorizado. Por favor, inicie sessão novamente.';
    case ApiStatusCode.Forbidden:
      return 'Não tem permissão para acessar a este recurso.';
    case ApiStatusCode.NotFound:
      return 'Recurso não encontrado.';
    case ApiStatusCode.BadRequest:
      return `Pedido inválido ao acessar ${target}.`;
    case ApiStatusCode.InternalServerError:
      return 'Erro interno do servidor. Tente novamente mais tarde.';
    default:
      return `Não foi possível comunicar com o servidor ao acessar ${target}.`;
  }
}

export function parseApiError(err: unknown, resource?: string): ApiErrorResult {
  if (isContractViolation(err)) {
    return { kind: 'contract', message: CONTRACT_ERROR_MESSAGE, domainError: null, fieldErrors: NO_FIELD_ERRORS };
  }

  if (!isAxiosError(err)) {
    return { kind: 'unknown', message: UNKNOWN_ERROR_MESSAGE, domainError: null, fieldErrors: NO_FIELD_ERRORS };
  }

  const status = err.response?.status ?? 500;
  const domainError = tryParseDomainError(err.response?.data);

  if (domainError) {
    const { message, fieldErrors } = breakdownDomainError(domainError, resource);

    return {
      kind: 'domain',
      message,
      statusCode: domainError.statusCode ?? status,
      domainError,
      correlationId: domainError.correlationId,
      fieldErrors
    };
  }

  return {
    kind: 'http',
    message: defaultHttpStatusMessage(status, resource),
    statusCode: status,
    domainError: null,
    fieldErrors: NO_FIELD_ERRORS
  };
}
