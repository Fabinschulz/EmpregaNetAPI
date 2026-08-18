import { z } from 'zod';

export const createdIdResponseSchema = z.number().int().positive();
export type CreatedId = z.infer<typeof createdIdResponseSchema>;
