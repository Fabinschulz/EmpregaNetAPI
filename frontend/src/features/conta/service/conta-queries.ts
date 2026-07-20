'use client';

import { useAuth } from '@/context';
import { saveSessionMetadata } from '@/shared/auth';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { changeMyPassword, deleteMyAccount, me, updateMyProfile } from './conta-api';
import { contaKeys } from './conta-keys';
import type { ChangeMyPasswordFormValues, ProfileFormValues } from './conta-schema';

export function useMeQuery() {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: contaKeys.me(),
    queryFn: () => me(),
    enabled: isAuthenticated
  });
}

/** Atualiza os dados da própria conta e mantém o cache/menu coerentes com o novo perfil. */
export function useUpdateMyProfileMutation() {
  const queryClient = useQueryClient();
  const { roles } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ProfileFormValues) => updateMyProfile(formValue),
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

/**
 * Altera a própria senha. O backend revoga todas as sessões (todos os dispositivos);
 * Sessão local também é encerrada e o usuário volta ao login.
 */
export function useChangeMyPasswordMutation() {
  const { logout } = useAuth();
  const router = useRouter();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ChangeMyPasswordFormValues) => changeMyPassword(formValue),
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
