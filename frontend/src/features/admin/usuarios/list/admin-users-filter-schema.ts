import {
  LIST_ORDER_BY_VALUES,
  LIST_SEARCH_MAX_LENGTH,
  LIST_SEARCH_MAX_LENGTH_MESSAGE,
  type AdminUsersListQueryParams
} from '@/shared/schema';
import { USER_TYPES } from '@/shared/utils/lib/user-types';
import { z } from 'zod';

export const adminUsersUserTypeFilterValues = ['all', ...USER_TYPES.map((type) => type.value)] as const;

export const adminUsersFilterFormSchema = z.object({
  search: z.string().trim().max(LIST_SEARCH_MAX_LENGTH, { message: LIST_SEARCH_MAX_LENGTH_MESSAGE }),
  situation: z.enum(['all', 'active', 'deleted']),
  userType: z.enum(adminUsersUserTypeFilterValues),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type AdminUsersFilterFormValues = z.infer<typeof adminUsersFilterFormSchema>;

export const defaultAdminUsersFilter: AdminUsersFilterFormValues = {
  search: '',
  situation: 'all',
  userType: 'all',
  orderBy: 'createdAt_DESC'
};

export type AdminUsersFilterParams = Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'userType' | 'orderBy'>;

export function adminUsersFilterToParams(values: AdminUsersFilterFormValues): AdminUsersFilterParams {
  return {
    search: values.search.trim() || undefined,
    isDeleted: values.situation === 'all' ? undefined : values.situation === 'deleted',
    userType: values.userType === 'all' ? undefined : values.userType,
    orderBy: values.orderBy
  };
}
