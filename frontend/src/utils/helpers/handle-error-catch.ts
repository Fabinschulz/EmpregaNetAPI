import { isAxiosError } from 'axios';
import { ApiStatusCode } from '../enums';

export type ApiErrorResult = {
  message: string;
  statusCode?: number;
};

const UNKNOWN_ERROR_MESSAGE =
  'Erro desconhecido. Por favor, entre em contato com o suporte técnico.';

/**
 * Interpreta erros Axios (e desconhecidos) e devolve mensagem + status HTTP quando existir.
 * Função pura — preferir esta em React Query (derivar de `error` no render).
 */
export function parseApiError(err: unknown, resource?: string): ApiErrorResult {
  if (!isAxiosError(err)) return { message: UNKNOWN_ERROR_MESSAGE };

  const { response } = err;
  const status = response?.status;
  const error = {
    code: `${status ?? ''} ${response?.statusText ?? ''}`.trim(),
    message: (response?.data as { message?: string } | undefined)?.message
  };

  switch (status) {
    case ApiStatusCode.BadRequest:
      return {
        message: `${error.code} - Ops! Algo deu errado. Por favor, tente novamente.`,
        statusCode: status
      };
    case ApiStatusCode.Unauthorized:
      return {
        message: `${error.code} - Acesso não autorizado. Por favor, realize a autenticação novamente.`,
        statusCode: status
      };
    case ApiStatusCode.Forbidden:
      return {
        message: `${error.code} - Usuário não tem permissão para acessar este recurso`,
        statusCode: status
      };
    case ApiStatusCode.NotFound:
      return {
        message: `${error.code} - Recurso não encontrado`,
        statusCode: status
      };
    case ApiStatusCode.InternalServerError:
      return {
        message: `${error.code} - Erro interno do servidor`,
        statusCode: status
      };
    default:
      return {
        message: `${ApiStatusCode.InternalServerError} - Ops! Ocorreu um erro interno do servidor ao acessar ${resource ?? 'o recurso'}!`,
        statusCode: status ?? ApiStatusCode.InternalServerError
      };
  }
}