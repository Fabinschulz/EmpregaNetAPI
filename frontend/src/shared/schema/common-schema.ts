import { z } from 'zod';

export const domainErrorItemSchema = z.object({
  field: z.string().nullish(),
  message: z.string(),
  code: z
    .union([z.string(), z.number().int()])
    .optional()
    .transform((value) => (value === undefined ? undefined : String(value)))
});

export type DomainErrorItemResponse = z.infer<typeof domainErrorItemSchema>;

/** Corpo JSON de erro devolvido pela API (`DomainError` do backend). */
export const domainErrorSchema = z.object({
  statusCode: z.number().int().optional(),
  code: z
    .union([z.string(), z.number().int()])
    .optional()
    .transform((value) => (value === undefined ? undefined : String(value))),
  message: z.string().optional(),
  errors: z.array(domainErrorItemSchema).optional(),
  correlationId: z.string().optional(),
  stackTrace: z.string().nullish()
});

export type DomainErrorResponse = z.infer<typeof domainErrorSchema>;
