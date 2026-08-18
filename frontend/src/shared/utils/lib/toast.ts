import { toast } from 'sonner';
import { parseApiError, type ApiErrorResult } from '../errors/parse-api-error';

const SUCCESS_DURATION_MS = 4_500;
const ERROR_DURATION_MS = 7_000;
const INFO_DURATION_MS = 5_000;

export function toastSuccess(title: string, description?: string): void {
  toast.success(title, { description, duration: SUCCESS_DURATION_MS });
}

export function toastError(title: string, description?: string): void {
  toast.error(title, { description, duration: ERROR_DURATION_MS });
}

export function toastInfo(title: string, description?: string): void {
  toast.info(title, { description, duration: INFO_DURATION_MS });
}

/** Aceita tanto o erro cru como um resultado já classificado, para não parsear duas vezes. */
function isApiErrorResult(value: unknown): value is ApiErrorResult {
  return (
    typeof value === 'object' &&
    value !== null &&
    !(value instanceof Error) &&
    'kind' in value &&
    'message' in value &&
    'domainError' in value
  );
}

/**
 * Feedback uniforme para falhas de API ou de contrato.
 *
 * <para>O `correlationId` só aparece quando é acionável: em falha do servidor ou quebra de
 * contrato é o que o suporte pede para localizar o log. Nos restantes casos seria ruído.</para>
 */
export function notifyApiError(err: unknown, actionLabel: string, resource?: string): ApiErrorResult {
  const result = isApiErrorResult(err) ? err : parseApiError(err, resource);

  const title = result.kind === 'contract' ? 'Resposta inesperada do servidor' : `Não foi possível ${actionLabel}`;
  const showCorrelation = (result.kind === 'server' || result.kind === 'contract') && result.correlationId;
  const description = showCorrelation ? `${result.message} (ref. ${result.correlationId})` : result.message;

  toastError(title, description);

  console.error(`[api] ${actionLabel} falhou`, {
    kind: result.kind,
    statusCode: result.statusCode,
    correlationId: result.correlationId,
    code: result.domainError?.code,
    endpoint: result.endpoint,
    resource
  });

  return result;
}
