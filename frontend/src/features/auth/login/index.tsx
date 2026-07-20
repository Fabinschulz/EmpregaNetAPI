'use client';

import { FormProvider } from '@/context';
import { useLoginMutation, useLoginWithGoogleMutation } from '../service';
import { useEffect, useRef } from 'react';
import {
  StandalonePageDivider,
  StandalonePageFooterPrompt,
  StandalonePageFormActions,
  StandalonePageLinkRow,
  StandalonePageNavLink,
  StandalonePage
} from '@/components';
import { GoogleSignInButton } from '../shared';
import { LoginFormFields } from './login-form';
import type { LoginDto } from './login-schema';
import { loginDefaultValues, loginSchema } from './login-schema';

export function Login() {
  const { apiError, mutateAsync, isPending, resetFeedback } = useLoginMutation();
  const googleMutation = useLoginWithGoogleMutation();
  const { resetFeedback: resetGoogleFeedback } = googleMutation;

  const clearedStaleFeedbackRef = useRef(false);
  useEffect(() => {
    if (clearedStaleFeedbackRef.current) return;
    clearedStaleFeedbackRef.current = true;
    resetFeedback();
    resetGoogleFeedback();
  }, [resetFeedback, resetGoogleFeedback]);

  const handleSubmit = async (formValue: LoginDto) => await mutateAsync(formValue);
  const handleGoogleCredential = (idToken: string) => {
    void googleMutation.mutateAsync({ idToken });
  };

  const displayError = apiError ?? googleMutation.apiError;
  const isBusy = isPending || googleMutation.isPending;

  return (
    <StandalonePage
      title="Acesse sua conta"
      apiError={displayError}
      footer={
        <StandalonePageFooterPrompt prompt="Não tem uma conta?">
          <StandalonePageNavLink href="/register">Inscrever-se</StandalonePageNavLink>
        </StandalonePageFooterPrompt>
      }
    >
      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <LoginFormFields />
        <StandalonePageLinkRow>
          <StandalonePageNavLink href="/forgot-password" muted>
            Esqueceu a senha?
          </StandalonePageNavLink>
          <StandalonePageNavLink href="/resend-confirmation" muted>
            Reenviar confirmação de e-mail
          </StandalonePageNavLink>
        </StandalonePageLinkRow>
      </FormProvider>

      <StandalonePageDivider />

      <StandalonePageFormActions>
        <GoogleSignInButton onCredential={handleGoogleCredential} disabled={isBusy} />
      </StandalonePageFormActions>
    </StandalonePage>
  );
}
