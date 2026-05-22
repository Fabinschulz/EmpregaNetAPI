import { z } from 'zod';
import { listDataPaginationSchema } from '../shared';

export const jobSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  description: z.string().nullable().optional(),
  location: z.string().nullable().optional(),
  companyId: z.number().int().nullable().optional(),
  isActive: z.boolean().optional()
});

export type JobDto = z.infer<typeof jobSchema>;
export const jobsListResponseSchema = listDataPaginationSchema(jobSchema);
export type JobsListResponseDto = z.infer<typeof jobsListResponseSchema>;

export const jobFormSchema = z.object({
  title: z.string().min(1, 'Informe o título.'),
  description: z.string(),
  location: z.string()
});

export type JobFormValues = z.infer<typeof jobFormSchema>;
