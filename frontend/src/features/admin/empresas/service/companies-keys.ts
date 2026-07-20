import type { ListQueryParams } from '@/shared/schema';

/** Query keys do domínio de empresas (fonte única para cache/invalidação). */
export const companiesKeys = {
  all: ['companies'] as const,
  lists: () => [...companiesKeys.all, 'list'] as const,
  list: (params: ListQueryParams) => [...companiesKeys.lists(), params] as const,
  details: () => [...companiesKeys.all, 'detail'] as const,
  detail: (id: number) => [...companiesKeys.details(), id] as const
};
