'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, startRouterTransition } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { requireAuthToken, withDefaultListParams, type AdminUsersListQueryParams } from '../shared';
import { deleteAdminUser, getAdminUser, listAdminUsers, updateAdminUser } from './admin-api';
import type { AdminUserUpdateFormValues } from './admin-schema';

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

export function useUpdateAdminUserMutation(userId: number) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: AdminUserUpdateFormValues) =>
      updateAdminUser(requireAuthToken(token), userId, {
        userType: formValue.userType.trim() || null
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.detail(userId) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.lists() });
      startRouterTransition(() => router.push('/admin/usuarios'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteAdminUserMutation(userId: number) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => deleteAdminUser(requireAuthToken(token), userId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers.all });
      startRouterTransition(() => router.push('/admin/usuarios'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}
