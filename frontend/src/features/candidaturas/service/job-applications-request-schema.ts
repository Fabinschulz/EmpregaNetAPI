import { z } from 'zod';
import { applicationStatusSchema } from '../domain/application-status';

export const applyToJobRequestSchema = z.object({
  jobId: z.number().int().positive({ message: '"jobId" deve ser um id válido.' })
});

export const changeApplicationStatusRequestSchema = z.object({
  status: applicationStatusSchema
});

export type ApplyToJobRequest = z.infer<typeof applyToJobRequestSchema>;
export type ChangeApplicationStatusRequest = z.infer<typeof changeApplicationStatusRequestSchema>;
