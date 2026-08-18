import { LIST_ORDER_BY_VALUES, type JobApplicationsListQueryParams } from '@/shared/schema';
import { z } from 'zod';
import { APPLICATION_STATUSES } from '../domain';

export const myApplicationsFilterFormSchema = z.object({
  status: z.enum(['all', ...APPLICATION_STATUSES]),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type MyApplicationsFilterFormValues = z.infer<typeof myApplicationsFilterFormSchema>;

export const defaultMyApplicationsFilter: MyApplicationsFilterFormValues = {
  status: 'all',
  orderBy: 'createdAt_DESC'
};

export function myApplicationsFilterToParams(
  values: MyApplicationsFilterFormValues
): Pick<JobApplicationsListQueryParams, 'status' | 'orderBy'> {
  return {
    status: values.status === 'all' ? undefined : values.status,
    orderBy: values.orderBy
  };
}
