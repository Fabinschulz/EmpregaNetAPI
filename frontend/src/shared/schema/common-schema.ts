import { z } from 'zod';

/** Corpo JSON de erro devolvido pela API (`DomainError` do backend). */
export const domainErrorSchema = z.object({
  statusCode: z.number().int().optional(),
  code: z
    .union([z.string(), z.number().int()])
    .optional()
    .transform((value) => (value === undefined ? undefined : String(value))),
  message: z.string().optional(),
  correlationId: z.string().optional(),
  details: z.unknown().optional()
});

export type DomainErrorDto = z.infer<typeof domainErrorSchema>;
