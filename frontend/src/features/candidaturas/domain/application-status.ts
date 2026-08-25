import { Ban, CheckCircle2, Flag, PlayCircle, RotateCcw, XCircle, type LucideIcon } from 'lucide-react';
import { z } from 'zod';

export const APPLICATION_STATUSES = [
  'Pending',
  'Processing',
  'Approved',
  'Rejected',
  'Timeout',
  'Canceled',
  'Error',
  'Finished'
] as const;

export const applicationStatusSchema = z.enum(APPLICATION_STATUSES);

export type ApplicationStatus = z.infer<typeof applicationStatusSchema>;

export function parseApplicationStatus(status: string | null | undefined): ApplicationStatus | null {
  const parsed = applicationStatusSchema.safeParse(status);
  return parsed.success ? parsed.data : null;
}

export const applicationStatusLabels: Record<ApplicationStatus, string> = {
  Pending: 'Recebida',
  Processing: 'Em análise',
  Approved: 'Aprovada',
  Rejected: 'Reprovada',
  Timeout: 'Prazo expirado',
  Canceled: 'Cancelada',
  Error: 'Erro',
  Finished: 'Concluída'
};

export const applicationStatusTransitions: Record<ApplicationStatus, ApplicationStatus[]> = {
  Pending: ['Processing', 'Canceled'],
  Processing: ['Approved', 'Rejected', 'Canceled'],
  Approved: ['Finished'],
  Rejected: [],
  Timeout: [],
  Canceled: [],
  Error: [],
  Finished: []
};

export const applicationTransitionLabels: Record<ApplicationStatus, string> = {
  Pending: 'Reabrir',
  Processing: 'Iniciar análise',
  Approved: 'Aprovar',
  Rejected: 'Reprovar',
  Timeout: 'Expirar',
  Canceled: 'Cancelar',
  Error: 'Marcar erro',
  Finished: 'Concluir'
};

export const applicationTransitionIcons: Record<ApplicationStatus, LucideIcon> = {
  Pending: RotateCcw,
  Processing: PlayCircle,
  Approved: CheckCircle2,
  Rejected: XCircle,
  Canceled: Ban,
  Finished: Flag,
  Timeout: Ban,
  Error: Ban
};
