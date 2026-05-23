import { z } from 'zod';
import { listDataPaginationSchema } from '../shared';
import { userSchema, type UserDto } from '../users/users-schema';

export const adminUsersListResponseSchema = listDataPaginationSchema(userSchema);
export type AdminUsersListResponseDto = z.infer<typeof adminUsersListResponseSchema>;

export const adminUserUpdateFormSchema = z.object({
  userType: z.string()
});

export type AdminUserUpdateFormValues = z.infer<typeof adminUserUpdateFormSchema>;

export const defaultAdminUserUpdateForm: AdminUserUpdateFormValues = {
  userType: ''
};

export function adminUserFormValuesFromDto(user: UserDto): AdminUserUpdateFormValues {
  return { userType: user.userType ?? '' };
}
