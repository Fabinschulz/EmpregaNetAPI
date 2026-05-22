import { z } from 'zod';
import { listDataPaginationSchema } from '../shared';
import { userSchema } from '../users/users-schema';

export const candidatesListResponseSchema = listDataPaginationSchema(userSchema);
export type CandidatesListResponseDto = z.infer<typeof candidatesListResponseSchema>;
