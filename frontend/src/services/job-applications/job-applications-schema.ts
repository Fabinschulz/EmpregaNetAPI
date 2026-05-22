import { z } from 'zod';
import { listDataPaginationSchema } from '../shared';

export const jobApplicationSchema = z.object({
  id: z.number().int(),
  jobId: z.number().int().optional(),
  candidateId: z.number().int().optional(),
  status: z.string().nullable().optional(),
  createdAt: z.string().datetime().optional()
});

export type JobApplicationDto = z.infer<typeof jobApplicationSchema>;
export const jobApplicationsListResponseSchema = listDataPaginationSchema(jobApplicationSchema);
export type JobApplicationsListResponseDto = z.infer<typeof jobApplicationsListResponseSchema>;
