import { z } from 'zod';

export const paginationSchema = z.object({
  page: z.number().int().positive(),
  size: z.number().int().positive(),
  total: z.number().int().nonnegative().optional(),
  totalPages: z.number().int().nonnegative().optional()
});

export const listDataPaginationSchema = <T extends z.ZodTypeAny>(t: T) =>
  z.object({
    data: z.array(t),
    page: z.number().int().positive().optional(),
    size: z.number().int().positive().optional(),
    total: z.number().int().nonnegative().optional(),
    totalPages: z.number().int().nonnegative().optional()
  });

export const domainErrorSchema = z.object({
  statusCode: z.number().int().optional(),
  code: z.string().optional(),
  message: z.string().optional(),
  correlationId: z.string().optional(),
  details: z.unknown().optional()
});

export type DomainErrorDto = z.infer<typeof domainErrorSchema>;
