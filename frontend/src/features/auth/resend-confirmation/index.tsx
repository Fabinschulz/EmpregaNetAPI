'use client';

import { FormProvider } from '@/context';
import { useResendEmailConfirmationMutation } from '../service';
import { StandalonePageNavLink, StandalonePage } from '@/components';
import { ResendConfirmationFormFields } from './resend-confirmation-form';
import type { ResendEmailConfirmationDto } from './resend-confirmation-schema';
import { resendConfirmationDefaultValues, resendEmailConfirmationSchema } from './resend-confirmation-schema';

export function ResendConfirmation() {
  const { apiError, mutateAsync, successMessage } = useResendEmailConfirmationMutation();
  const handleSubmit = async (formValue: ResendEmailConfirmationDto) => await mutateAsync(formValue);

  return (
    <StandalonePage
      title="Reenviar confirmação"
      description="Se o e-mail existir e ainda não estiver confirmado, enviaremos um novo link."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          <StandalonePageNavLink href="/login">Voltar ao login</StandalonePageNavLink>
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
    </StandalonePage>
  );
}
