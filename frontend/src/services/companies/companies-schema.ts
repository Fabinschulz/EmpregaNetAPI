import { z } from 'zod';
import { listDataPaginationSchema } from '../shared';

export const companySchema = z.object({
  id: z.number().int(),
  name: z.string().min(1),
  documentNo: z.string().nullable().optional(),
  email: z.string().email().nullable().optional(),
  phone: z.string().nullable().optional()
});

export type CompanyDto = z.infer<typeof companySchema>;
export const companiesListResponseSchema = listDataPaginationSchema(companySchema);
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

export const defaultFormCompany: CompanyFormValues = {
  name: '',
  email: '',
  phone: '',
  documentNo: ''
};

export function companyFormValuesFromDto(company: CompanyDto): CompanyFormValues {
  return {
    name: company.name,
    email: company.email ?? '',
    phone: company.phone ?? '',
    documentNo: company.documentNo ?? ''
  };
}
