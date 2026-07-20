import { userSchema } from '@/shared/auth';
import { LIST_ORDER_BY_VALUES, createPaginatedResponseSchema, type CandidatesListQueryParams } from '@/shared/schema';
import { z } from 'zod';

export const candidatesListResponseSchema = createPaginatedResponseSchema(userSchema);
export type CandidatesListResponseDto = z.infer<typeof candidatesListResponseSchema>;

export const candidatesFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type CandidatesFilterFormValues = z.infer<typeof candidatesFilterFormSchema>;

export const defaultCandidatesFilter: CandidatesFilterFormValues = {
  search: '',
  orderBy: 'createdAt_DESC'
};

export function candidatesFilterToParams(
  values: CandidatesFilterFormValues
): Pick<CandidatesListQueryParams, 'search' | 'orderBy'> {
  return {
    search: values.search.trim() || undefined,
    orderBy: values.orderBy
  };
}
