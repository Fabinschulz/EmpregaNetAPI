import { LIST_ORDER_BY_VALUES, type CompaniesListQueryParams } from '@/shared/schema';
import { z } from 'zod';

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
