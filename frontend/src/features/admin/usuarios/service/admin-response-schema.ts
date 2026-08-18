import { createPaginatedResponseSchema, userResponseSchema } from '@/shared/schema';
import { z } from 'zod';

export const adminUsersListResponseSchema = createPaginatedResponseSchema(userResponseSchema);
export type AdminUsersListResponse = z.infer<typeof adminUsersListResponseSchema>;
