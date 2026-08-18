import type { JobsListQueryParams } from '@/shared/schema';
import { z } from 'zod';

export const jobsFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  status: z.enum(['all', 'active', 'closed'])
});

export type JobsFilterFormValues = z.infer<typeof jobsFilterFormSchema>;

export const defaultJobsFilter: JobsFilterFormValues = {
  search: '',
  status: 'all'
};

export function jobsFilterToParams(values: JobsFilterFormValues): Pick<JobsListQueryParams, 'search' | 'isActive'> {
  return {
    search: values.search.trim() || undefined,
    isActive: values.status === 'all' ? undefined : values.status === 'active'
  };
}
