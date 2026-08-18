import { createPaginatedResponseSchema } from '@/shared/schema';
import { z } from 'zod';

const jobApplicationCandidateResponseSchema = z.object({
  id: z.number().int(),
  name: z.string(),
  email: z.string(),
  isDeleted: z.boolean()
});

export type JobApplicationCandidateResponse = z.infer<typeof jobApplicationCandidateResponseSchema>;

export const jobApplicationResponseSchema = z.object({
  id: z.number().int(),
  jobId: z.number().int(),
  candidate: jobApplicationCandidateResponseSchema,
  status: z.string(),
  appliedAt: z.string(),
  createdAt: z.string().nullable().optional(),
  updatedAt: z.string().nullable().optional(),
  deletedAt: z.string().nullable().optional(),
  isDeleted: z.boolean().optional()
});

export type JobApplicationResponse = z.infer<typeof jobApplicationResponseSchema>;

export const jobApplicationsListResponseSchema = createPaginatedResponseSchema(jobApplicationResponseSchema);

export type JobApplicationsListResponse = z.infer<typeof jobApplicationsListResponseSchema>;

export function candidateDisplayName(candidate: JobApplicationCandidateResponse): string {
  return candidate.name.trim() || `#${candidate.id}`;
}
