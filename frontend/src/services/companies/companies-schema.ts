import { z } from 'zod';

export const companySchema = z.object({
  id: z.number().int(),
  name: z.string().min(1),
  documentNo: z.string().nullable().optional(),
  email: z.string().email().nullable().optional(),
  phone: z.string().nullable().optional()
});

export const companiesListResponseSchema = z.object({
  data: z.array(companySchema),
  page: z.number().optional(),
  size: z.number().optional(),
  total: z.number().optional(),
  totalPages: z.number().optional()
});

export type CompanyDto = z.infer<typeof companySchema>;
export type CompaniesListResponseDto = z.infer<typeof companiesListResponseSchema>;

export const companyFormSchema = z.object({
  name: z.string().min(1, 'Informe o nome.'),
  email: z
    .string()
    .trim()
    .refine((s) => s === '' || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(s), { message: 'E-mail inválido.' }),
  phone: z.string(),
  documentNo: z.string()
});

export type CompanyFormValues = z.infer<typeof companyFormSchema>;
