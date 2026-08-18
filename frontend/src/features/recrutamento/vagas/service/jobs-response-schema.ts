import { createPaginatedResponseSchema } from '@/shared/schema';
import { z } from 'zod';

export const jobResponseSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  summary: z.string().nullable().optional(),
  description: z.string(),
  companyId: z.number().int(),
  salaryMin: z.number().nullable().optional(),
  salaryMax: z.number().nullable().optional(),
  salaryDisclosed: z.boolean(),
  jobType: z.string(),
  workModel: z.string(),
  workShift: z.string(),
  experienceLevel: z.string(),
  area: z.string(),
  isPcdFriendly: z.boolean(),
  city: z.string(),
  state: z.string(),
  country: z.string(),
  requirements: z.array(z.string()),
  benefits: z.array(z.string()),
  isActive: z.boolean(),
  publicationDate: z.string(),
  publishedAt: z.string(),
  createdAt: z.string().nullable().optional(),
  updatedAt: z.string().nullable().optional(),
  deletedAt: z.string().nullable().optional(),
  isDeleted: z.boolean().optional()
});

export type JobResponse = z.infer<typeof jobResponseSchema>;
export const jobsListResponseSchema = createPaginatedResponseSchema(jobResponseSchema);
export type JobsListResponse = z.infer<typeof jobsListResponseSchema>;

const companyOptionResponseSchema = z.object({
  id: z.number().int(),
  name: z.string()
});

export const companyOptionsResponseSchema = z.array(companyOptionResponseSchema);
export type CompanyOption = z.infer<typeof companyOptionResponseSchema>;
