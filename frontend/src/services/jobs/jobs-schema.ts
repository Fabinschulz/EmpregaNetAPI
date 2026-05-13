import { z } from 'zod';

export const jobSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  description: z.string().nullable().optional(),
  location: z.string().nullable().optional(),
  companyId: z.number().int().nullable().optional(),
  isActive: z.boolean().optional()
});

export const jobsListResponseSchema = z.object({
  data: z.array(jobSchema),
  page: z.number().optional(),
  size: z.number().optional(),
  total: z.number().optional(),
  totalPages: z.number().optional()
});

export type JobDto = z.infer<typeof jobSchema>;
export type JobsListResponseDto = z.infer<typeof jobsListResponseSchema>;

export const jobFormSchema = z.object({
  title: z.string().min(1, 'Informe o título.'),
  description: z.string(),
  location: z.string()
});

export type JobFormValues = z.infer<typeof jobFormSchema>;
