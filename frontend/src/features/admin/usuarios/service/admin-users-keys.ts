import type { ListQueryParams } from '@/shared/schema';

/** Query keys do domínio de usuários (admin). */
export const adminUsersKeys = {
  all: ['admin-users'] as const,
  lists: () => [...adminUsersKeys.all, 'list'] as const,
  list: (params: ListQueryParams) => [...adminUsersKeys.lists(), params] as const,
  details: () => [...adminUsersKeys.all, 'detail'] as const,
  detail: (id: number) => [...adminUsersKeys.details(), id] as const
};
