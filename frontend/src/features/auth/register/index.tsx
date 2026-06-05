'use client';

import { FormProvider } from '@/context';
import { useRegisterMutation } from '@/services';
import { AuthNavLink, AuthPage } from '../shared';
import { RegisterFormFields } from './register-form';
import type { RegisterDto } from './register-schema';
import { registerDefaultValues, registerFormSchema } from './register-schema';

export function Register() {
  const { apiError, mutateAsync, successMessage } = useRegisterMutation();
  const handleSubmit = async (formValue: RegisterDto) => await mutateAsync(formValue);

  return (
    <AuthPage
      title="Criar conta"
      description="Enviaremos um e-mail para confirmar o endereço antes do primeiro acesso."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          Não recebeu o e-mail? <AuthNavLink href="/resend-confirmation">Reenviar confirmação</AuthNavLink>
          <br />
          Já tem conta? <AuthNavLink href="/login">Entrar</AuthNavLink>
        </>
      }
    >
      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <RegisterFormFields />
      </FormProvider>
    </AuthPage>
  );
}
