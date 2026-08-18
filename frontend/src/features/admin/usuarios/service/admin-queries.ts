'use client';

import { useAuth } from '@/shared/context';
import { withDefaultListParams, type AdminUsersListQueryParams } from '@/shared/schema';
import { reportMutationApiError, toastSuccess } from '@/shared/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { adminUserFormToRequest, type AdminUserUpdateFormValues } from '../detail/admin-user-form-schema';
import { deleteAdminUser, getAdminUser, listAdminUsers, updateAdminUser } from './admin-api';
import { adminUsersKeys } from './admin-users-keys';

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

  const ctx = useMutation({
    mutationFn: (formValue: AdminUserUpdateFormValues) => updateAdminUser(userId, adminUserFormToRequest(formValue)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.detail(userId) });
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.lists() });
      toastSuccess('Usuário atualizado', 'As alterações foram gravadas.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteAdminUserMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (id: number) => deleteAdminUser(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
      toastSuccess('Usuário excluído', 'O usuário foi marcado como excluído.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir usuário', resource: 'usuário', setApiError });
    }
  });

  return { ...ctx, apiError };
}
