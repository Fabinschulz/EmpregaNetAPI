'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, resolvePostLoginPath, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useCallback, useState } from 'react';
import { queryKeys } from '../query-keys';
import { requireAuthToken } from '../shared';
import {
  confirmEmail,
  forgotPassword,
  login,
  loginWithGoogle,
  me,
  register,
  resendEmailConfirmation,
  resetPassword
} from './users-public-api';
import type {
  ConfirmEmailDto,
  ForgotPasswordDto,
  LoginDto,
  LoginWithGoogleDto,
  RegisterDto,
  ResendEmailConfirmationDto,
  ResetPasswordFormValues
} from './users-schema';

function useAuthSessionMutation(actionLabel: string, resource: string) {
  const { setLoggedUser } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const router = useRouter();

  const resetFeedback = useCallback(() => {
    setApiError(null);
    setSuccessMessage(null);
  }, []);

  return {
    setLoggedUser,
    apiError,
    setApiError,
    successMessage,
    setSuccessMessage,
    resetFeedback,
    router,
    onMutate: () => {
      setApiError(null);
      setSuccessMessage(null);
    },
    onAuthSuccess: (res: Parameters<typeof setLoggedUser>[0]) => {
      setApiError(null);
      setSuccessMessage(null);
      setLoggedUser(res);
      toastSuccess('Sessão iniciada com sucesso', 'Bem-vindo à EmpregaUAI.');

      const params = typeof window !== 'undefined' ? new URLSearchParams(window.location.search) : null;
      startRouterTransition(() => router.replace(resolvePostLoginPath(params)));
    },
    onAuthError: (err: unknown) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel, resource, setApiError });
    }
  };
}

export function useMeQuery() {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.users.me(),
    queryFn: () => me(requireAuthToken(token)),
    enabled: !!token
  });
}

export function useLoginMutation() {
  const auth = useAuthSessionMutation('iniciar sessão', 'sessão');

  const ctx = useMutation({
    mutationFn: (formValue: LoginDto) => login(formValue),
    onMutate: auth.onMutate,
    onSuccess: auth.onAuthSuccess,
    onError: auth.onAuthError
  });

  return { ...ctx, apiError: auth.apiError, successMessage: auth.successMessage, resetFeedback: auth.resetFeedback };
}

export function useLoginWithGoogleMutation() {
  const auth = useAuthSessionMutation('iniciar sessão com Google', 'sessão');

  const ctx = useMutation({
    mutationFn: (formValue: LoginWithGoogleDto) => loginWithGoogle(formValue),
    onMutate: auth.onMutate,
    onSuccess: auth.onAuthSuccess,
    onError: auth.onAuthError
  });

  return { ...ctx, apiError: auth.apiError, resetFeedback: auth.resetFeedback };
}

export function useRegisterMutation() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: RegisterDto) => register(formValue),
    onSuccess: (res) => {
      setApiError(null);
      const message =
        typeof res === 'string' ? res : 'Conta criada. Confirme o e-mail antes de iniciar sessão.';
      setSuccessMessage(message);
      toastSuccess('Registo concluído', message);
    },
    onError: (err) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel: 'criar conta', resource: 'conta', setApiError });
    }
  });

  return { ...ctx, apiError, successMessage };
}

export function useForgotPasswordMutation() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ForgotPasswordDto) => forgotPassword(formValue),
    onSuccess: (message) => {
      setApiError(null);
      setSuccessMessage(message);
      toastSuccess('Pedido enviado', message);
    },
    onError: (err) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel: 'recuperar senha', resource: 'senha', setApiError });
    }
  });

  return { ...ctx, apiError, successMessage };
}

export function useResetPasswordMutation() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: ResetPasswordFormValues) => resetPassword(formValue),
    onSuccess: (message) => {
      setApiError(null);
      setSuccessMessage(message);
      toastSuccess('Senha atualizada', message);
      startRouterTransition(() => router.push('/login'));
    },
    onError: (err) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel: 'redefinir senha', resource: 'senha', setApiError });
    }
  });

  return { ...ctx, apiError, successMessage };
}

export function useConfirmEmailMutation() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: ConfirmEmailDto) => confirmEmail(formValue),
    onSuccess: (message) => {
      setApiError(null);
      setSuccessMessage(message);
      toastSuccess('E-mail confirmado', message);
      startRouterTransition(() => router.push('/login'));
    },
    onError: (err) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel: 'confirmar e-mail', resource: 'e-mail', setApiError });
    }
  });

  return { ...ctx, apiError, successMessage };
}

export function useResendEmailConfirmationMutation() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ResendEmailConfirmationDto) => resendEmailConfirmation(formValue),
    onSuccess: (message) => {
      setApiError(null);
      setSuccessMessage(message);
      toastSuccess('E-mail reenviado', message);
    },
    onError: (err) => {
      setSuccessMessage(null);
      reportMutationApiError({ err, actionLabel: 'reenviar confirmação', resource: 'e-mail', setApiError });
    }
  });

  return { ...ctx, apiError, successMessage };
}
