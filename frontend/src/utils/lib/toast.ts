import { parseApiError } from '@/utils';
import { isAxiosError } from 'axios';
import { toast } from 'sonner';
import { z } from 'zod';

const GENERIC =
  'Não foi possível concluir a operação neste momento. Verifique a sua conexão com à rede e tente novamente dentro de instantes.';

export function toastSuccess(title: string, description?: string): void {
  toast.success(title, description !== undefined ? { description, duration: 4500 } : { duration: 4500 });
}

export function toastError(title: string, description?: string): void {
  toast.error(title, description !== undefined ? { description, duration: 7000 } : { duration: 7000 });
}

export function toastInfo(title: string, description?: string): void {
  toast.info(title, description !== undefined ? { description, duration: 5000 } : { duration: 5000 });
}

/**
 * Feedback uniforme para falhas de API ou validação na UI (Zod).
 * `resource` aparece na descrição quando útil (ex.: nome da área).
 */
export function notifyApiError(err: unknown,   resource: string, actionLabel: string): void {
  const suffix = resource ? ` (${resource})` : '';

  if (err instanceof z.ZodError) {
    toast.error('Dados inválidos ou incompletos', {
      description: `A resposta do servidor não pôde ser validada. Se o problema persistir, contacte o suporte.`
    });
    return;
  }

  if (isAxiosError(err)) {
    const parsed = parseApiError(err, resource);
      toast.error(`Erro ao ${actionLabel}`, { description: parsed.message, duration: 7000 });
    return;
  }

  if (err instanceof Error && err.message.trim().length > 0) {
    toast.error('Não foi possível concluir a ação', {
      description: `${err.message.trim()}${suffix}`
    });
    return;
  }

  toast.error('Ocorreu um erro inesperado', { description: `${GENERIC}${suffix}` });
}
