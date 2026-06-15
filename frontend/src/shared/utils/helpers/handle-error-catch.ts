import { type DomainErrorDto } from '@/services/shared/common-schema';
import { isAxiosError } from 'axios';
import { ApiStatusCode } from '../enums';
import { formatDomainErrorMessage, tryParseDomainError } from '../errors/domain-error';

export type ApiErrorResult = {
  message: string;
  statusCode?: number;
  domainError: DomainErrorDto | null;
  correlationId?: string;
};

const UNKNOWN_ERROR_MESSAGE = 'Erro desconhecido. Por favor, entre em contato com o suporte técnico.';

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

/**
 * Interpreta erros Axios (corpo `DomainError` quando existir) e desconhecidos.
 */
export function parseApiError(err: unknown, resource?: string): ApiErrorResult {
  if (!isAxiosError(err)) {
    return { message: UNKNOWN_ERROR_MESSAGE, domainError: null };
  }

  const status = err.response?.status ?? 500;
  const domainError = tryParseDomainError(err.response?.data);

  if (domainError) {
    return {
      message: formatDomainErrorMessage(domainError, resource),
      statusCode: domainError.statusCode ?? status,
      domainError,
      correlationId: domainError.correlationId
    };
  }

  return {
    message: defaultHttpStatusMessage(status, resource),
    statusCode: status,
    domainError: null
  };
}
