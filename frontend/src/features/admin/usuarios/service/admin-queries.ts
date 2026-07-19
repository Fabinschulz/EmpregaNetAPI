'use client';

import { useAuth } from '@/context';
import { withDefaultListParams, type AdminUsersListQueryParams } from '@/shared';
import { reportMutationApiError, startRouterTransition } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { adminUsersKeys } from './admin-users-keys';
import { deleteAdminUser, getAdminUser, listAdminUsers, updateAdminUser } from './admin-api';
import type { AdminUserUpdateFormValues } from './admin-schema';

export function useAdminUsersListQuery(params?: AdminUsersListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: adminUsersKeys.list(listParams),
    queryFn: () => listAdminUsers(listParams),
    enabled: isAuthenticated
  });
}

export function useAdminUserQuery(id: number) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: adminUsersKeys.detail(id),
    queryFn: () => getAdminUser(id),
    enabled: isAuthenticated && Number.isFinite(id) && id > 0
  });
}

export function useUpdateAdminUserMutation(userId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: AdminUserUpdateFormValues) =>
      updateAdminUser(userId, {
        userType: formValue.userType.trim() || null
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.detail(userId) });
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.lists() });
      startRouterTransition(() => router.push('/admin/usuarios'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteAdminUserMutation(userId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => deleteAdminUser(userId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
      startRouterTransition(() => router.push('/admin/usuarios'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}
