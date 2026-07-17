import { LIST_ORDER_BY_VALUES, createPaginatedResponseSchema, type CompaniesListQueryParams } from '@/shared';
import { z } from 'zod';

export const companySchema = z.object({
  id: z.number().int(),
  name: z.string().min(1),
  documentNo: z.string().nullable().optional(),
  email: z.string().email().nullable().optional(),
  phone: z.string().nullable().optional()
});

export type CompanyDto = z.infer<typeof companySchema>;
export const companiesListResponseSchema = createPaginatedResponseSchema(companySchema);
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

export const companiesFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  situation: z.enum(['all', 'active', 'deleted']),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type CompaniesFilterFormValues = z.infer<typeof companiesFilterFormSchema>;

export const defaultCompaniesFilter: CompaniesFilterFormValues = {
  search: '',
  situation: 'all',
  orderBy: 'createdAt_DESC'
};

export function companiesFilterToParams(
  values: CompaniesFilterFormValues
): Pick<CompaniesListQueryParams, 'search' | 'isDeleted' | 'orderBy'> {
  return {
    search: values.search.trim() || undefined,
    isDeleted: values.situation === 'all' ? undefined : values.situation === 'deleted',
    orderBy: values.orderBy
  };
}
