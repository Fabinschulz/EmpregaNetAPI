import { parseApiError, type ApiErrorResult } from './handle-error-catch';

export type { ApiErrorResult };

/** Mensagem padronizada (sem expor status). Use `parseApiError` ou `useQueryApiError` quando precisar do código HTTP. */
export function getApiErrorMessage(err: unknown, resource?: string): string {
  return parseApiError(err, resource).message;
}
