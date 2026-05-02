import { z } from "zod";
import { userSchema } from "../users/users-schema";

export const adminUsersListResponseSchema = z.object({
  data: z.array(userSchema),
  page: z.number().optional(),
  size: z.number().optional(),
  total: z.number().optional(),
  totalPages: z.number().optional(),
});

export type AdminUsersListResponseDto = z.infer<typeof adminUsersListResponseSchema>;

export const adminUserUpdateFormSchema = z.object({
  userType: z.string(),
});

export type AdminUserUpdateFormValues = z.infer<typeof adminUserUpdateFormSchema>;
