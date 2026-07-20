import type { ListQueryParams } from '@/shared/schema';

/** Query keys do domínio de vagas (compartilhadas pelo catálogo público e recrutamento). */
export const jobsKeys = {
  all: ['jobs'] as const,
  lists: () => [...jobsKeys.all, 'list'] as const,
  list: (params: ListQueryParams) => [...jobsKeys.lists(), params] as const,
  details: () => [...jobsKeys.all, 'detail'] as const,
  detail: (id: number) => [...jobsKeys.details(), id] as const,
  selectableCompanies: () => [...jobsKeys.all, 'selectable-companies'] as const
};
