import { domainErrorSchema, type DomainErrorResponse } from '@/shared/schema';

const UNKNOWN_ERROR_MESSAGE = 'Erro desconhecido. Por favor, entre em contato com o suporte técnico.';

export type FieldErrorMap = Readonly<Record<string, string>>;

export type DomainErrorBreakdown = {
  message: string;
  fieldErrors: FieldErrorMap;
};

/**
 * Valida o corpo JSON de erro devolvido pela API (`DomainError`).
 */
export function tryParseDomainError(payload: unknown): DomainErrorResponse | null {
  const parsed = domainErrorSchema.safeParse(payload);
  return parsed.success ? parsed.data : null;
}

/**
 * Separa as falhas entre as que pertencem a um campo e as que pertencem ao formulário.
 *
 * <para>A API marca as segundas com `field: null`; essas entram na mensagem do topo, que a
 * página já exibe.</para>
 */
export function breakdownDomainError(domainError: DomainErrorResponse, resource?: string): DomainErrorBreakdown {
  const fieldErrors: Record<string, string> = {};
  const formErrors: string[] = [];

  for (const item of domainError.errors ?? []) {
    const message = item.message.trim();
    if (!message) continue;

    const field = item.field?.trim();

    if (!field) {
      formErrors.push(message);
    } else if (!(field in fieldErrors)) {
      fieldErrors[field] = message;
    }
  }

  return { message: resolveMessage(domainError, formErrors, resource), fieldErrors };
}

export function pickKnownFields(fieldErrors: FieldErrorMap, isKnownField: (field: string) => boolean): FieldErrorMap {
  return Object.fromEntries(Object.entries(fieldErrors).filter(([field]) => isKnownField(field)));
}

function resolveMessage(domainError: DomainErrorResponse, formErrors: readonly string[], resource?: string): string {
  const primary = domainError.message?.trim();

  if (primary) {
    const extras = formErrors.filter((message) => message !== primary);
    return extras.length > 0 ? `${primary} ${extras.join(' ')}` : primary;
  }

  if (formErrors.length > 0) return formErrors.join(' ');

  if (domainError.code) {
    return resource
      ? `Não foi possível concluir a operação em ${resource} (${domainError.code}).`
      : `Não foi possível concluir a operação (${domainError.code}).`;
  }

  return resource ? `Ocorreu um erro ao acessar ${resource}.` : UNKNOWN_ERROR_MESSAGE;
}
