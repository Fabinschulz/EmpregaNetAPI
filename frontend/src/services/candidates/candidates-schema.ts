import { z } from 'zod';
import { userSchema } from '../users/users-schema';

export const candidatesListResponseSchema = z.object({
  data: z.array(userSchema),
  page: z.number().optional(),
  size: z.number().optional(),
  total: z.number().optional(),
  totalPages: z.number().optional()
});

export type CandidatesListResponseDto = z.infer<typeof candidatesListResponseSchema>;
