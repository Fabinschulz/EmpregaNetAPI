import type { UserResponse } from '@/shared/schema';
import { normalizeUserTypeValue } from '@/shared/utils';
import { z } from 'zod';
import type { UpdateAdminUserRequest } from '../service/admin-request-schema';

export const adminUserUpdateFormSchema = z.object({
  userType: z.string().min(1, { message: 'Selecione o tipo de usuário.' })
});

export type AdminUserUpdateFormValues = z.infer<typeof adminUserUpdateFormSchema>;
export const defaultAdminUserUpdateForm: AdminUserUpdateFormValues = {
  userType: ''
};

export function adminUserFormValuesFromResponse(user: UserResponse): AdminUserUpdateFormValues {
  return { userType: normalizeUserTypeValue(user.userType) };
}

export function adminUserFormToRequest(values: AdminUserUpdateFormValues): UpdateAdminUserRequest {
  return { userType: values.userType };
}
