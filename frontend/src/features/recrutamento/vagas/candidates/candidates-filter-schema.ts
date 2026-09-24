import { APPLICATION_STATUSES } from '@/features/candidaturas/domain';
import {
  LIST_ORDER_BY_VALUES,
  LIST_SEARCH_MAX_LENGTH,
  LIST_SEARCH_MAX_LENGTH_MESSAGE,
  type JobApplicationsListQueryParams
} from '@/shared/schema';
import { z } from 'zod';

export const candidatesStatusFilterValues = ['all', ...APPLICATION_STATUSES] as const;

export const candidatesFilterSchema = z.object({
  status: z.enum(candidatesStatusFilterValues),
  search: z.string().trim().max(LIST_SEARCH_MAX_LENGTH, { message: LIST_SEARCH_MAX_LENGTH_MESSAGE }),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type CandidatesFilterFormValues = z.infer<typeof candidatesFilterSchema>;

export const defaultCandidatesFilter: CandidatesFilterFormValues = {
  status: 'all',
  search: '',
  orderBy: 'createdAt_DESC'
};

export type CandidatesFilterParams = Pick<JobApplicationsListQueryParams, 'status' | 'search' | 'orderBy'>;

export function candidatesFilterToParams(values: CandidatesFilterFormValues): CandidatesFilterParams {
  return {
    status: values.status === 'all' ? undefined : values.status,
    search: values.search.trim() || undefined,
    orderBy: values.orderBy
  };
}
