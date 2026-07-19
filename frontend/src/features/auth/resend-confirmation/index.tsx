'use client';

import { FormProvider } from '@/context';
import { useResendEmailConfirmationMutation } from '../service';
import { AuthNavLink, AuthPage } from '../shared';
import { ResendConfirmationFormFields } from './resend-confirmation-form';
import type { ResendEmailConfirmationDto } from './resend-confirmation-schema';
import { resendConfirmationDefaultValues, resendEmailConfirmationSchema } from './resend-confirmation-schema';

export function ResendConfirmation() {
  const { apiError, mutateAsync, successMessage } = useResendEmailConfirmationMutation();
  const handleSubmit = async (formValue: ResendEmailConfirmationDto) => await mutateAsync(formValue);

  return (
    <AuthPage
      title="Reenviar confirmação"
      description="Se o e-mail existir e ainda não estiver confirmado, enviaremos um novo link."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          <AuthNavLink href="/login">Voltar ao login</AuthNavLink>
        </>
      }
    >
      <FormProvider
        validationSchema={resendEmailConfirmationSchema}
        defaultValues={resendConfirmationDefaultValues}
        onSubmit={handleSubmit}
      >
        <ResendConfirmationFormFields />
      </FormProvider>
    </AuthPage>
  );
}
