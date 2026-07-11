import { createPaginatedResponseSchema } from '@/shared';
import { z } from 'zod';
import { userSchema } from '../users/users-schema';

export const candidatesListResponseSchema = createPaginatedResponseSchema(userSchema);
export type CandidatesListResponseDto = z.infer<typeof candidatesListResponseSchema>;
