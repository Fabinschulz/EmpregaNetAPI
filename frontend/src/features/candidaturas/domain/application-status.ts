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
  'Finished',
  'CanceledByCandidate'
] as const;

export const applicationStatusSchema = z.enum(APPLICATION_STATUSES);

export type ApplicationStatus = z.infer<typeof applicationStatusSchema>;
export type ApplicationStatusAudience = 'candidate' | 'recruiter';

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
  Finished: 'Concluída',
  CanceledByCandidate: 'Cancelada pelo candidato'
};

const candidateStatusLabelOverrides: Partial<Record<ApplicationStatus, string>> = {
  CanceledByCandidate: 'Cancelada por você'
};

export function applicationStatusLabel(status: ApplicationStatus, audience: ApplicationStatusAudience): string {
  if (audience === 'candidate') {
    return candidateStatusLabelOverrides[status] ?? applicationStatusLabels[status];
  }

  return applicationStatusLabels[status];
}

export const applicationStatusTransitions: Record<ApplicationStatus, ApplicationStatus[]> = {
  Pending: ['Processing', 'Canceled'],
  Processing: ['Approved', 'Rejected', 'Canceled'],
  Approved: ['Finished'],
  Rejected: [],
  Timeout: [],
  Canceled: [],
  Error: [],
  Finished: [],
  CanceledByCandidate: []
};

export const applicationTransitionLabels: Record<ApplicationStatus, string> = {
  Pending: 'Reabrir',
  Processing: 'Iniciar análise',
  Approved: 'Aprovar',
  Rejected: 'Reprovar',
  Timeout: 'Expirar',
  Canceled: 'Cancelar',
  Error: 'Marcar erro',
  Finished: 'Concluir',
  CanceledByCandidate: 'Cancelar pelo candidato'
};

export const applicationTransitionIcons: Record<ApplicationStatus, LucideIcon> = {
  Pending: RotateCcw,
  Processing: PlayCircle,
  Approved: CheckCircle2,
  Rejected: XCircle,
  Canceled: Ban,
  Finished: Flag,
  Timeout: Ban,
  Error: Ban,
  CanceledByCandidate: Ban
};

/** Estados em que o candidato ainda pode cancelar a própria candidatura. */
export const CANDIDATE_CANCELABLE_STATUSES: readonly ApplicationStatus[] = ['Pending', 'Processing'];

export function canCandidateCancelApplication(status: string | null | undefined): boolean {
  const parsed = parseApplicationStatus(status);
  return parsed !== null && CANDIDATE_CANCELABLE_STATUSES.includes(parsed);
}
