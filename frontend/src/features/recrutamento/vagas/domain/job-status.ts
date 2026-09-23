import { z } from 'zod';

export const JOB_STATUSES = ['Active', 'ClosedManually', 'ClosedByFulfillment'] as const;
export const jobStatusSchema = z.enum(JOB_STATUSES);

export type JobStatus = z.infer<typeof jobStatusSchema>;
export type JobStatusAudience = 'candidate' | 'recruiter';

export const jobStatusResponseSchema = jobStatusSchema.catch('ClosedManually');

export const jobStatusLabels: Record<JobStatus, string> = {
  Active: 'Ativa',
  ClosedManually: 'Encerrada pela empresa',
  ClosedByFulfillment: 'Vagas preenchidas'
};

const candidateStatusLabelOverrides: Partial<Record<JobStatus, string>> = {
  ClosedManually: 'Encerrada'
};

export function jobStatusLabel(status: JobStatus, audience: JobStatusAudience = 'recruiter'): string {
  if (audience === 'candidate') {
    return candidateStatusLabelOverrides[status] ?? jobStatusLabels[status];
  }

  return jobStatusLabels[status];
}

export function jobStatusTone(status: JobStatus): 'positive' | 'negative' | 'neutral' {
  if (status === 'Active') return 'positive';
  if (status === 'ClosedByFulfillment') return 'neutral';
  return 'negative';
}

export const DELETED_JOB_STATUS_LABEL = 'Excluída';

type JobStatusSummary = { status: JobStatus; isDeleted?: boolean | null };

export function describeJobStatusBadge(
  job: JobStatusSummary,
  audience: JobStatusAudience = 'recruiter'
): { label: string; tone: 'positive' | 'negative' | 'neutral' } {
  if (job.isDeleted) {
    return { label: DELETED_JOB_STATUS_LABEL, tone: 'neutral' };
  }
  return { label: jobStatusLabel(job.status, audience), tone: jobStatusTone(job.status) };
}

/** Uma vaga excluída (soft delete) não pode mais ser editada nem excluída de novo. */
export function canManageJob(job: { isDeleted?: boolean | null }): boolean {
  return !job.isDeleted;
}

/** "3 de 5 vagas preenchidas" - a frase que responde às três quantidades de uma vez. */
export function describePositions(positions: number, filledPositions: number): string {
  return `${filledPositions} de ${positions} ${positions === 1 ? 'vaga preenchida' : 'vagas preenchidas'}`;
}

/** "Restam 2 vagas" / "Resta 1 vaga" / "Nenhuma vaga disponível". */
export function describeAvailablePositions(availablePositions: number): string {
  if (availablePositions <= 0) return 'Nenhuma vaga disponível';
  if (availablePositions === 1) return 'Resta 1 vaga';
  return `Restam ${availablePositions} vagas`;
}
