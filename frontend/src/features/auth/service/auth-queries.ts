'use client';

import { useAuth } from '@/shared/context';
import { reportMutationApiError, resolvePostLoginPath, startRouterTransition, toastSuccess } from '@/shared/utils';
import { useMutation } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useCallback, useState } from 'react';
import {
  confirmEmail,
  forgotPassword,
  login,
  loginWithGoogle,
  register,
  resendEmailConfirmation,
  resetPassword
} from './auth-api';
import { forgotPasswordFormToRequest, type ForgotPasswordFormValues } from '../forgot-password/forgot-password-schema';
import { loginFormToRequest, type LoginFormValues } from '../login/login-schema';
import { registerFormToRequest, type RegisterFormValues } from '../register/register-schema';
import {
  resendConfirmationFormToRequest,
  type ResendConfirmationFormValues
} from '../resend-confirmation/resend-confirmation-schema';
import { resetPasswordFormToRequest, type ResetPasswordFormValues } from '../reset-password/reset-password-schema';
import type { ConfirmEmailRequest, LoginWithGoogleRequest } from './auth-request-schema';

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

export function useLoginMutation() {
  const auth = useAuthSessionMutation('iniciar sessão', 'sessão');

  const ctx = useMutation({
    mutationFn: (formValue: LoginFormValues) => login(loginFormToRequest(formValue)),
    onMutate: auth.onMutate,
    onSuccess: auth.onAuthSuccess,
    onError: auth.onAuthError
  });

  return { ...ctx, apiError: auth.apiError, successMessage: auth.successMessage, resetFeedback: auth.resetFeedback };
}

export function useLoginWithGoogleMutation() {
  const auth = useAuthSessionMutation('iniciar sessão com Google', 'sessão');

  const ctx = useMutation({
    mutationFn: (request: LoginWithGoogleRequest) => loginWithGoogle(request),
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
    mutationFn: (formValue: RegisterFormValues) => register(registerFormToRequest(formValue)),
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
    mutationFn: (formValue: ForgotPasswordFormValues) => forgotPassword(forgotPasswordFormToRequest(formValue)),
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
    mutationFn: (formValue: ResetPasswordFormValues) => resetPassword(resetPasswordFormToRequest(formValue)),
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
    mutationFn: (request: ConfirmEmailRequest) => confirmEmail(request),
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
    mutationFn: (formValue: ResendConfirmationFormValues) =>
      resendEmailConfirmation(resendConfirmationFormToRequest(formValue)),
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
