import { z } from 'zod';

export const jobApplicationSchema = z.object({
  id: z.number().int(),
  jobId: z.number().int().optional(),
  candidateId: z.number().int().optional(),
  status: z.string().nullable().optional(),
  createdAt: z.string().datetime().optional()
});

export const jobApplicationsListResponseSchema = z.object({
  data: z.array(jobApplicationSchema),
  page: z.number().optional(),
  size: z.number().optional(),
  total: z.number().optional(),
  totalPages: z.number().optional()
});

export type JobApplicationDto = z.infer<typeof jobApplicationSchema>;
export type JobApplicationsListResponseDto = z.infer<typeof jobApplicationsListResponseSchema>;
