import { parseApiError, type ApiErrorResult } from './handle-error-catch';

export type { ApiErrorResult };

/** Mensagem padronizada (sem expor status). Use `parseApiError` ou `useQueryApiError` quando precisar do código HTTP. */
export function getApiErrorMessage(err: unknown, resource?: string): string {
  return parseApiError(err, resource).message;
}

/** Status HTTP do erro Axios, quando aplicável. */
export function getApiErrorStatusCode(err: unknown, resource?: string): number | undefined {
  return parseApiError(err, resource).statusCode;
}

/** Mensagem + status HTTP num único objeto. */
export function getApiError(err: unknown, resource?: string): ApiErrorResult {
  return parseApiError(err, resource);
}
