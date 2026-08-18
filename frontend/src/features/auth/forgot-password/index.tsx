'use client';

import { FormProvider } from '@/context';
import { useForgotPasswordMutation } from '../service';
import { StandalonePageNavLink, StandalonePage } from '@/components';
import { ForgotPasswordFormFields } from './forgot-password-form';
import {
  forgotPasswordDefaultValues,
  forgotPasswordFormSchema,
  type ForgotPasswordFormValues
} from './forgot-password-schema';

export function ForgotPassword() {
  const { apiError, mutateAsync, successMessage } = useForgotPasswordMutation();
  const handleSubmit = async (formValue: ForgotPasswordFormValues) => await mutateAsync(formValue);

  return (
    <StandalonePage
      title="Recuperar senha"
      description="Indique o e-mail da conta. Se existir, receberá instruções para redefinir a senha."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          <StandalonePageNavLink href="/login">Voltar ao login</StandalonePageNavLink>
        </>
      }
    >
      <FormProvider
        validationSchema={forgotPasswordFormSchema}
        defaultValues={forgotPasswordDefaultValues}
        onSubmit={handleSubmit}
      >
        <ForgotPasswordFormFields />
      </FormProvider>
    </StandalonePage>
  );
}
