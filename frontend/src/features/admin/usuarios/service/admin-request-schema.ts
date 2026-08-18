import { USER_TYPES } from '@/utils';
import { z } from 'zod';

const USER_TYPE_VALUE_SET: ReadonlySet<string> = new Set(USER_TYPES.map((t) => t.value));

export const updateAdminUserRequestSchema = z.object({
  userType: z.string().refine((value) => USER_TYPE_VALUE_SET.has(value), {
    message: '"userType" deve ser o nome do enum aceito pela API (ex.: "Candidate"), não o rótulo pt-BR.'
  })
});

export type UpdateAdminUserRequest = z.infer<typeof updateAdminUserRequestSchema>;
