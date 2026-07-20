import { domainErrorSchema, type DomainErrorDto } from '@/shared/schema';

const UNKNOWN_ERROR_MESSAGE = 'Erro desconhecido. Por favor, entre em contato com o suporte técnico.';

/**
 * Valida o corpo JSON de erro devolvido pela API (`DomainError`).
 */
export function tryParseDomainError(payload: unknown): DomainErrorDto | null {
  const parsed = domainErrorSchema.safeParse(payload);
  return parsed.success ? parsed.data : null;
}

/** Extrai mensagens de `details.Errors` (validação e exceções de domínio). */
function _extractDomainErrorDetailMessages(details: unknown): string[] {
  if (!details || typeof details !== 'object') return [];

  const record = details as Record<string, unknown>;
  const raw = record.Errors ?? record.errors;
  if (!Array.isArray(raw)) return [];

  return raw
    .map((item) => (typeof item === 'string' ? item.trim() : String(item ?? '').trim()))
    .filter((item) => item.length > 0);
}

/**
 * Mensagem para UI a partir de um `DomainError` já validado.
 */
export function formatDomainErrorMessage(domainError: DomainErrorDto, resource?: string): string {
  const detailMessages = _extractDomainErrorDetailMessages(domainError.details);
  const primary = domainError.message?.trim();

  if (detailMessages.length > 0) {
    if (primary) return `${primary} ${detailMessages.join(' ')}`;
    return detailMessages.join('. ');
  }

  if (primary) return primary;

  if (domainError.code) {
    return resource
      ? `Não foi possível concluir a operação em ${resource} (${domainError.code}).`
      : `Não foi possível concluir a operação (${domainError.code}).`;
  }

  return resource ? `Ocorreu um erro ao acessar ${resource}.` : UNKNOWN_ERROR_MESSAGE;
}
