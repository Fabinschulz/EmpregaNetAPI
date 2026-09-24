import {
  LIST_ORDER_BY_VALUES,
  LIST_SEARCH_MAX_LENGTH,
  LIST_SEARCH_MAX_LENGTH_MESSAGE,
  type JobsListQueryParams
} from '@/shared/schema';
import { z } from 'zod';

export const jobsFilterFormSchema = z.object({
  search: z.string().trim().max(LIST_SEARCH_MAX_LENGTH, { message: LIST_SEARCH_MAX_LENGTH_MESSAGE }),
  status: z.enum(['all', 'active', 'closed']),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type JobsFilterFormValues = z.infer<typeof jobsFilterFormSchema>;

export const defaultJobsFilter: JobsFilterFormValues = {
  search: '',
  status: 'all',
  orderBy: 'createdAt_DESC'
};

export type JobsFilterParams = Pick<JobsListQueryParams, 'search' | 'isActive' | 'orderBy'>;

export function jobsFilterToParams(values: JobsFilterFormValues): JobsFilterParams {
  return {
    search: values.search.trim() || undefined,
    isActive: values.status === 'all' ? undefined : values.status === 'active',
    orderBy: values.orderBy
  };
}
