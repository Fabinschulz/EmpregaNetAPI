'use client';

import { FormProvider } from '@/shared/context';
import { useLoginMutation, useLoginWithGoogleMutation } from '../service';
import { useEffect, useRef } from 'react';
import {
  StandalonePageDivider,
  StandalonePageFooterPrompt,
  StandalonePageFormActions,
  StandalonePageLinkRow,
  StandalonePageNavLink,
  StandalonePage
} from '@/shared/components';
import { GoogleSignInButton } from '../shared';
import { LoginFormFields } from './login-form';
import { loginDefaultValues, loginFormSchema, type LoginFormValues } from './login-schema';

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

  const handleSubmit = async (formValue: LoginFormValues) => await mutateAsync(formValue);
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
      <FormProvider validationSchema={loginFormSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
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
