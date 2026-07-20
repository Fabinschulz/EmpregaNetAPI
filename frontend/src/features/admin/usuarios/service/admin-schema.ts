import { userSchema, type UserDto } from '@/shared/auth';
import { LIST_ORDER_BY_VALUES, createPaginatedResponseSchema, type AdminUsersListQueryParams } from '@/shared/schema';
import { normalizeUserTypeValue } from '@/utils';
import { z } from 'zod';

export const adminUsersListResponseSchema = createPaginatedResponseSchema(userSchema);
export type AdminUsersListResponseDto = z.infer<typeof adminUsersListResponseSchema>;

export const adminUserUpdateFormSchema = z.object({
  userType: z.string()
});

export type AdminUserUpdateFormValues = z.infer<typeof adminUserUpdateFormSchema>;

export const defaultAdminUserUpdateForm: AdminUserUpdateFormValues = {
  userType: ''
};

export function adminUserFormValuesFromDto(user: UserDto): AdminUserUpdateFormValues {
  return { userType: normalizeUserTypeValue(user.userType) };
}

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
