import { z } from 'zod';

export const messageResponseSchema = z.object({
  message: z.string()
});

export function readMessageOr(data: unknown, fallback: string): string {
  const parsed = messageResponseSchema.safeParse(data);
  return parsed.success ? parsed.data.message : fallback;
}
