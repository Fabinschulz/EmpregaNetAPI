import { createPaginatedResponseSchema, type JobsListQueryParams } from '@/shared';
import { z } from 'zod';

export const jobSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  description: z.string().nullable().optional(),
  location: z.string().nullable().optional(),
  companyId: z.number().int().nullable().optional(),
  isActive: z.boolean().optional()
});

export type JobDto = z.infer<typeof jobSchema>;
export const jobsListResponseSchema = createPaginatedResponseSchema(jobSchema);
export type JobsListResponseDto = z.infer<typeof jobsListResponseSchema>;

export const jobFormSchema = z.object({
  title: z.string().min(1, 'Informe o título.'),
  description: z.string(),
  location: z.string()
});

export type JobFormValues = z.infer<typeof jobFormSchema>;

export const defaultFormJob: JobFormValues = {
  title: '',
  description: '',
  location: ''
};

export function jobFormValuesFromDto(job: JobDto): JobFormValues {
  return {
    title: job.title,
    description: job.description ?? '',
    location: job.location ?? ''
  };
}

// --- Filtros da tabela de vagas ---
export const jobsFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  status: z.enum(['all', 'active', 'closed'])
});

export type JobsFilterFormValues = z.infer<typeof jobsFilterFormSchema>;
export const defaultJobsFilter: JobsFilterFormValues = {
  search: '',
  status: 'all'
};

/** Converte os valores do formulário de filtro nos parâmetros aceitos pela API de vagas. */
export function jobsFilterToParams(values: JobsFilterFormValues): Pick<JobsListQueryParams, 'search' | 'isActive'> {
  return {
    search: values.search.trim() || undefined,
    isActive: values.status === 'all' ? undefined : values.status === 'active'
  };
}

/** Busca do catálogo público de vagas (visitante só vê vagas ativas; sem filtro de situação). */
export const jobsSearchFormSchema = jobsFilterFormSchema.pick({ search: true });

export type JobsSearchFormValues = z.infer<typeof jobsSearchFormSchema>;

export const defaultJobsSearch: JobsSearchFormValues = { search: '' };
