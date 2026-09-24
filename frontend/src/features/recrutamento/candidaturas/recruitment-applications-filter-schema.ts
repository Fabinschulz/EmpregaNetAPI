import { APPLICATION_STATUSES } from '@/features/candidaturas/domain';
import {
  LIST_ORDER_BY_VALUES,
  LIST_SEARCH_MAX_LENGTH,
  LIST_SEARCH_MAX_LENGTH_MESSAGE,
  type JobApplicationsAdminListQueryParams
} from '@/shared/schema';
import { z } from 'zod';

export const recruitmentApplicationsStatusFilterValues = ['all', ...APPLICATION_STATUSES] as const;

export const recruitmentApplicationsFilterFormSchema = z.object({
  status: z.enum(recruitmentApplicationsStatusFilterValues),
  search: z.string().trim().max(LIST_SEARCH_MAX_LENGTH, { message: LIST_SEARCH_MAX_LENGTH_MESSAGE }),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type RecruitmentApplicationsFilterFormValues = z.infer<typeof recruitmentApplicationsFilterFormSchema>;

export const defaultRecruitmentApplicationsFilter: RecruitmentApplicationsFilterFormValues = {
  status: 'all',
  search: '',
  orderBy: 'createdAt_DESC'
};

export type RecruitmentApplicationsFilterParams = Pick<
  JobApplicationsAdminListQueryParams,
  'status' | 'search' | 'orderBy'
>;

export function recruitmentApplicationsFilterToParams(
  values: RecruitmentApplicationsFilterFormValues
): RecruitmentApplicationsFilterParams {
  return {
    status: values.status === 'all' ? undefined : values.status,
    search: values.search.trim() || undefined,
    orderBy: values.orderBy
  };
}
