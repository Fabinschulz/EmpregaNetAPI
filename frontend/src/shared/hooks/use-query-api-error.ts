import type { DomainErrorDto } from '@/shared/schema';
import { parseApiError } from '@/utils';

export type QueryApiErrorState = {
  message: string | null;
  statusCode: number | undefined;
  domainError: DomainErrorDto | null;
  correlationId?: string;
};

const EMPTY: QueryApiErrorState = {
  message: null,
  statusCode: undefined,
  domainError: null
};

/**
 * Deriva mensagem e metadados do `DomainError` a partir do `error` do TanStack Query.
 */
export function useQueryApiError(error: unknown, resource?: string): QueryApiErrorState {
  if (!error) return EMPTY;

  const parsed = parseApiError(error, resource);

  return {
    message: parsed.message,
    statusCode: parsed.statusCode,
    domainError: parsed.domainError,
    correlationId: parsed.correlationId
  };
}
