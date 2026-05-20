'use client';

import { useAuth } from '@/context';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import type { AuthIdMutationVars, AuthIdVars } from '../shared';
import { requireAuthToken, withDefaultListParams, type AdminUsersListQueryParams } from '../shared';
import { deleteAdminUser, getAdminUser, listAdminUsers, updateAdminUser } from './admin-api';

export function useAdminUsersListQuery(params?: AdminUsersListQueryParams) {
  const { token } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.adminUsers.list(listParams),
    queryFn: () => listAdminUsers(requireAuthToken(token), listParams),
    enabled: !!token
  });
}

export function useAdminUserQuery(id: number) {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.adminUsers.detail(id),
    queryFn: () => getAdminUser(requireAuthToken(token), id),
    enabled: !!token && Number.isFinite(id) && id > 0
  });
}

export function useUpdateAdminUserMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: AuthIdMutationVars) => updateAdminUser(requireAuthToken(token), id, dto),
    onSuccess: async (_data, { id }) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.detail(id) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.lists() });
    }
  });
}

export function useDeleteAdminUserMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: AuthIdVars) => deleteAdminUser(requireAuthToken(token), id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.all });
    }
  });
}
