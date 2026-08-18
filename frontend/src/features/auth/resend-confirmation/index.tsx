'use client';

import { FormProvider } from '@/shared/context';
import { useResendEmailConfirmationMutation } from '../service';
import { StandalonePageNavLink, StandalonePage } from '@/shared/components';
import { ResendConfirmationFormFields } from './resend-confirmation-form';
import {
  resendConfirmationDefaultValues,
  resendConfirmationFormSchema,
  type ResendConfirmationFormValues
} from './resend-confirmation-schema';

export function ResendConfirmation() {
  const { apiError, mutateAsync, successMessage } = useResendEmailConfirmationMutation();
  const handleSubmit = async (formValue: ResendConfirmationFormValues) => await mutateAsync(formValue);

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
        validationSchema={resendConfirmationFormSchema}
        defaultValues={resendConfirmationDefaultValues}
        onSubmit={handleSubmit}
      >
        <ResendConfirmationFormFields />
      </FormProvider>
    </StandalonePage>
  );
}
