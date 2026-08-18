import { LIST_ORDER_BY_VALUES, type AdminUsersListQueryParams } from '@/shared/schema';
import { z } from 'zod';

export const adminUsersFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  situation: z.enum(['all', 'active', 'deleted']),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type AdminUsersFilterFormValues = z.infer<typeof adminUsersFilterFormSchema>;

export const defaultAdminUsersFilter: AdminUsersFilterFormValues = {
  search: '',
  situation: 'all',
  orderBy: 'createdAt_DESC'
};

export function adminUsersFilterToParams(
  values: AdminUsersFilterFormValues
): Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'orderBy'> {
  return {
    search: values.search.trim() || undefined,
    isDeleted: values.situation === 'all' ? undefined : values.situation === 'deleted',
    orderBy: values.orderBy
  };
}
