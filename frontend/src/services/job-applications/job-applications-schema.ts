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

export const changeApplicationStatusSchema = z.object({
  status: z.string().min(1)
});
