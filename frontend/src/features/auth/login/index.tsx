'use client';

import { FormProvider } from '@/context';
import { useLoginMutation, useLoginWithGoogleMutation } from '@/services';
import {
    AuthDivider,
    AuthFormActions,
    AuthLinkRow,
    AuthNavLink,
    AuthPage,
    GoogleSignInButton
} from '../shared';
import { LoginFormFields } from './login-form';
import type { LoginDto } from './login-schema';
import { loginDefaultValues, loginSchema } from './login-schema';

export function Login() {
  const { apiError, mutateAsync, isPending } = useLoginMutation();
  const googleMutation = useLoginWithGoogleMutation();

  const handleSubmit = async (formValue: LoginDto) => await mutateAsync(formValue);
  const handleGoogleCredential = (idToken: string) => {
    void googleMutation.mutateAsync({ idToken });
  };

  const displayError = apiError ?? googleMutation.apiError;
  const isBusy = isPending || googleMutation.isPending;

  return (
    <AuthPage
      title="Entrar"
      description="Acesse com o seu e-mail ou conta Google."
      apiError={displayError}
    >
      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <LoginFormFields />
        <AuthLinkRow>
          <AuthNavLink href="/forgot-password">Esqueceu a senha?</AuthNavLink>
          <AuthNavLink href="/resend-confirmation">Reenviar confirmação de e-mail</AuthNavLink>
        </AuthLinkRow>
      </FormProvider>

      <AuthDivider />
      <AuthFormActions>
        <GoogleSignInButton onCredential={handleGoogleCredential} disabled={isBusy} />
      </AuthFormActions>

      <footer style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.9rem', color: 'var(--muted)' }}>
        Não tem conta? <AuthNavLink href="/register">Criar conta</AuthNavLink>
      </footer>
    </AuthPage>
  );
}
