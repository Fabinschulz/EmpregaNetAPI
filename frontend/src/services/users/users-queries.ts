'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { requireAuthToken } from '../shared';
import { login, me, register } from './users-public-api';
import type { LoginDto, RegisterDto } from './users-schema';

export function useMeQuery() {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.users.me(),
    queryFn: () => me(requireAuthToken(token)),
    enabled: !!token
  });
}

export function useLoginMutation() {
  const { setLoggedUser } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: LoginDto) => login(formValue),
    onSuccess: (res) => {
      setLoggedUser(res);
      toastSuccess('Sessão iniciada com sucesso', 'Bem-vindo de volta. Estamos a preparar o seu painel.');
      startRouterTransition(() => router.push('/dashboard'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'iniciar sessão', resource: 'sessão', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useRegisterMutation() {
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: RegisterDto) => register(formValue),
    onSuccess: (res) => {
      const message =
        typeof res === 'string' ? res : 'Conta criada. Confirme o endereço de e-mail antes de iniciar sessão.';
      toastSuccess('Registo concluído', message);
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'criar conta', resource: 'conta', setApiError });
    }
  });

  return { ...ctx, apiError };
}
