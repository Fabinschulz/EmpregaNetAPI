import { parseApiError } from '@/utils';

export type QueryApiErrorState = {
  message: string | null;
  statusCode: number | undefined;
};

const EMPTY: QueryApiErrorState = { message: null, statusCode: undefined };

/**
 * Deriva mensagem e status HTTP a partir do `error` do TanStack Query.
 * Não precisa de `useState` — o status vem do objeto de erro enquanto `isError` for true.
 */
export function useQueryApiError(error: unknown, resource?: string): QueryApiErrorState {
  if (!error) return EMPTY;
  const parsed = parseApiError(error, resource);
  
  return {
    message: parsed.message,
    statusCode: parsed.statusCode
  };
}
