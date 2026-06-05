'use client';

import { FormProvider } from '@/context';
import { useLoginMutation, useLoginWithGoogleMutation } from '@/services';
import {
  AuthDivider,
  AuthFooterPrompt,
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
      title="Acesse sua conta"
      apiError={displayError}
      footer={
        <AuthFooterPrompt prompt="Não tem uma conta?">
          <AuthNavLink href="/register">Inscrever-se</AuthNavLink>
        </AuthFooterPrompt>
      }
    >
      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <LoginFormFields />
        <AuthLinkRow>
          <AuthNavLink href="/forgot-password" muted>
            Esqueceu a senha?
          </AuthNavLink>
          <AuthNavLink href="/resend-confirmation" muted>
            Reenviar confirmação de e-mail
          </AuthNavLink>
        </AuthLinkRow>
      </FormProvider>

      <AuthDivider />

      <AuthFormActions>
        <GoogleSignInButton onCredential={handleGoogleCredential} disabled={isBusy} />
      </AuthFormActions>
    </AuthPage>
  );
}
