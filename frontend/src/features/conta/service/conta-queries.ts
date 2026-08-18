'use client';

import { useAuth } from '@/shared/context';
import { saveSessionMetadata } from '@/shared/auth';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/shared/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { profileFormToRequest, type ProfileFormValues } from '../perfil/profile-form-schema';
import {
  changeMyPasswordFormToRequest,
  type ChangeMyPasswordFormValues
} from '../seguranca/change-password-form-schema';
import { changeMyPassword, deleteMyAccount, me, updateMyProfile } from './conta-api';
import { contaKeys } from './conta-keys';

export function useMeQuery() {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: contaKeys.me(),
    queryFn: () => me(),
    enabled: isAuthenticated
  });
}

export function useUpdateMyProfileMutation() {
  const queryClient = useQueryClient();
  const { roles } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ProfileFormValues) => updateMyProfile(profileFormToRequest(formValue)),
    onSuccess: async (user) => {
      setApiError(null);
      await queryClient.invalidateQueries({ queryKey: contaKeys.me() });
      saveSessionMetadata({ roles, username: user.username, email: user.email });
      toastSuccess('Perfil atualizado', 'Os seus dados foram salvos.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar o perfil', resource: 'perfil', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useChangeMyPasswordMutation() {
  const { logout } = useAuth();
  const router = useRouter();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ChangeMyPasswordFormValues) => changeMyPassword(changeMyPasswordFormToRequest(formValue)),
    onSuccess: async (message) => {
      setApiError(null);
      toastSuccess('Senha alterada', `${message} Inicie sessão novamente.`);
      await logout();
      startRouterTransition(() => router.push('/login'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'alterar a senha', resource: 'senha', setApiError });
    }
  });

  return { ...ctx, apiError };
}

/** Encerra a própria conta (exclusão lógica) e finaliza a sessão local. */
export function useDeleteMyAccountMutation() {
  const { logout } = useAuth();
  const router = useRouter();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: () => deleteMyAccount(),
    onSuccess: async () => {
      setApiError(null);
      toastSuccess('Conta encerrada', 'A sua conta foi encerrada. Sentiremos a sua falta.');
      await logout();
      startRouterTransition(() => router.push('/login'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'encerrar a conta', resource: 'conta', setApiError });
    }
  });

  return { ...ctx, apiError };
}
