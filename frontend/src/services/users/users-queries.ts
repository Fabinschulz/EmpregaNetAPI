'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, resolvePostLoginPath, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useCallback, useState } from 'react';
import { queryKeys } from '../query-keys';
import { saveSessionMetadata } from './session';
import {
  changeMyPassword,
  confirmEmail,
  deleteMyAccount,
  forgotPassword,
  login,
  loginWithGoogle,
  me,
  register,
  resendEmailConfirmation,
  resetPassword,
  updateMyProfile
} from './users-public-api';
import type {
  ChangeMyPasswordFormValues,
  ConfirmEmailDto,
  ForgotPasswordDto,
  LoginDto,
  LoginWithGoogleDto,
  ProfileFormValues,
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
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: queryKeys.users.me(),
    queryFn: () => me(),
    enabled: isAuthenticated
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
      const message = typeof res === 'string' ? res : 'Conta criada. Confirme o e-mail antes de iniciar sessão.';
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

/** Atualiza os dados da própria conta e mantém o cache/menu coerentes com o novo perfil. */
export function useUpdateMyProfileMutation() {
  const queryClient = useQueryClient();
  const { roles } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: ProfileFormValues) => updateMyProfile(formValue),
    onSuccess: async (user) => {
      setApiError(null);
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.me() });
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
