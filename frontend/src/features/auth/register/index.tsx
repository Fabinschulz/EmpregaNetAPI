'use client';

import { FormProvider } from '@/context';
import { useRegisterMutation } from '../service';
import { StandalonePageNavLink, StandalonePage } from '@/components';
import { RegisterFormFields } from './register-form';
import type { RegisterDto } from './register-schema';
import { registerDefaultValues, registerFormSchema } from './register-schema';

export function Register() {
  const { apiError, mutateAsync, successMessage } = useRegisterMutation();
  const handleSubmit = async (formValue: RegisterDto) => await mutateAsync(formValue);

  return (
    <StandalonePage
      title="Criar conta"
      description="Enviaremos um e-mail para confirmar o endereço antes do primeiro acesso."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          Não recebeu o e-mail?{' '}
          <StandalonePageNavLink href="/resend-confirmation">Reenviar confirmação</StandalonePageNavLink>
          <br />
          Já tem conta? <StandalonePageNavLink href="/login">Entrar</StandalonePageNavLink>
        </>
      }
    >
      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <RegisterFormFields />
      </FormProvider>
    </StandalonePage>
  );
}
