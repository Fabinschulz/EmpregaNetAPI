import type { ListQueryParams } from '@/shared';

/** Query keys do domínio de candidatos. */
export const candidatesKeys = {
  all: ['candidates'] as const,
  lists: () => [...candidatesKeys.all, 'list'] as const,
  list: (params: ListQueryParams) => [...candidatesKeys.lists(), params] as const,
  details: () => [...candidatesKeys.all, 'detail'] as const,
  detail: (id: number) => [...candidatesKeys.details(), id] as const
};
