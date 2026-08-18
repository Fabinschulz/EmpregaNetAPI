import { createPaginatedResponseSchema, userResponseSchema } from '@/shared/schema';
import { z } from 'zod';

export const candidatesListResponseSchema = createPaginatedResponseSchema(userResponseSchema);
export type CandidatesListResponse = z.infer<typeof candidatesListResponseSchema>;
