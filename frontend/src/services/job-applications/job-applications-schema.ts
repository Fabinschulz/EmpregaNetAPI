import { createPaginatedResponseSchema } from '@/shared';
import { z } from 'zod';

export const jobApplicationSchema = z.object({
  id: z.number().int(),
  jobId: z.number().int().optional(),
  candidateId: z.number().int().optional(),
  status: z.string().nullable().optional(),
  createdAt: z.string().datetime().optional()
});

export type JobApplicationDto = z.infer<typeof jobApplicationSchema>;
export const jobApplicationsListResponseSchema = createPaginatedResponseSchema(jobApplicationSchema);
export type JobApplicationsListResponseDto = z.infer<typeof jobApplicationsListResponseSchema>;

export const applyToJobSchema = z.object({
  jobId: z.number().int().positive()
});

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

export const changeApplicationStatusSchema = z.object({
  status: applicationStatusSchema
});

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

/**
 * Transições oferecidas na UI a partir de cada status (curadoria do fluxo do processo
 * seletivo; a autorização e a validação final são sempre do backend). Status terminais
 * não oferecem transição.
 */
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

/** Rótulo do botão/ação que leva a candidatura para o status alvo. */
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

export function parseApplicationStatus(status: string | null | undefined): ApplicationStatus | null {
  const parsed = applicationStatusSchema.safeParse(status);
  return parsed.success ? parsed.data : null;
}
