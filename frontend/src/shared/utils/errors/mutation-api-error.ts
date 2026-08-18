import { notifyApiError } from '../lib/toast';
import { pickKnownFields } from './domain-error';
import { parseApiError, type ApiErrorResult } from './parse-api-error';

export type SetFieldError = (field: string, error: { message: string }) => void;

export function applyApiFieldErrors(
  result: ApiErrorResult,
  setFieldError: SetFieldError,
  isKnownField?: (field: string) => boolean
): number {
  const known = isKnownField ? pickKnownFields(result.fieldErrors, isKnownField) : result.fieldErrors;
  const entries = Object.entries(known);

  for (const [field, message] of entries) {
    setFieldError(field, { message });
  }

  return entries.length;
}

export type ReportMutationApiErrorOptions = {
  err: unknown;
  /** Verbo no infinitivo, para compor "Não foi possível <actionLabel>". Ex.: `'excluir vaga'`. */
  actionLabel: string;
  /** Substantivo do recurso, para as mensagens por status. Ex.: `'vaga'`. */
  resource?: string;
  /** Alimenta o `<Alert />` inline acima do formulário. */
  setApiError: (message: string | null) => void;
  /** `setError` do react-hook-form, quando a tela tiver formulário. */
  setFieldError?: SetFieldError;
  /**
   * Filtra os campos que este formulário sabe mostrar. Um erro dirigido a um campo inexistente
   * ficaria invisível ao utilizador; sem correspondência, a falha volta para a mensagem geral.
   */
  isKnownField?: (field: string) => boolean;
};

/**
 * Ponto único de reporte de falha de mutation.
 *
 * <para>Decide entre as duas formas de mostrar o erro: quando o servidor aponta campos que o
 * formulário conhece, a falha vai para junto dos campos (sem toast, que seria ruído duplicado);
 * caso contrário vira toast + `<Alert />` no topo. Concentrar a decisão aqui é o que impede cada
 * `onError:` de inventar a sua própria política.</para>
 */
export function reportMutationApiError({
  err,
  actionLabel,
  resource,
  setApiError,
  setFieldError,
  isKnownField
}: ReportMutationApiErrorOptions): ApiErrorResult {
  const result = parseApiError(err, resource);

  if (setFieldError && applyApiFieldErrors(result, setFieldError, isKnownField) > 0) {
    setApiError(null);
    return result;
  }

  notifyApiError(result, actionLabel, resource);
  setApiError(result.message);
  return result;
}
