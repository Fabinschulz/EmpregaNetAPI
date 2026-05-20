'use client';

import { Alert, InputField } from '@/components';
import { FormProvider } from '@/context';
import { useRegisterMutation } from '@/services';
import { notifyApiError, toastSuccess } from '@/utils';
import { useState } from 'react';
import type { RegisterDto } from './register-schema';
import { registerDefaultValues, registerFormSchema } from './register-schema';
import { RegisterSubmitButton } from './register-submit-button';

export function RegisterForm() {
  const [success, setSuccess] = useState<string | null>(null);
  const registerMutation = useRegisterMutation();

  async function handleSubmit(data: RegisterDto) {
    setSuccess(null);
    registerMutation.mutate(data, {
      onSuccess: (res) => {
        const message =
          typeof res === 'string' ? res : 'Conta criada. Confirme o endereço de e-mail antes de iniciar sessão.';
        setSuccess(message);
        toastSuccess('Registo concluído', message);
      },
      onError: (err) => {
        notifyApiError(err, 'Criação de conta');
      }
    });
  }

  return (
    <>
      {success ? (
        <Alert variant="success" title="Sucesso">
          {success}
        </Alert>
      ) : null}

      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <div>
          <InputField name="username" label="Nome de usuário" autoComplete="username" required />
          <InputField name="email" label="E-mail" type="email" autoComplete="email" required />
          <InputField name="password" label="Senha" type="password" autoComplete="new-password" required />
          <InputField
            name="passwordConfirmation"
            label="Confirmar senha"
            type="password"
            autoComplete="new-password"
            required
          />
          <RegisterSubmitButton />
        </div>
      </FormProvider>
    </>
  );
}
