'use client';

import { startTransition, useState } from 'react';
import { Alert, InputField } from '@/components';
import { FormProvider } from '@/context';
import { register } from '@/services';
import { notifyApiError, toastSuccess } from '@/utils/lib';
import { REGISTER_FIELDS_GRID_STYLE } from './constants';
import { registerDefaultValues, registerFormSchema } from './register-schema';
import type { RegisterDto } from './register-schema';
import { RegisterSubmitButton } from './register-submit-button';

export function RegisterForm() {
  const [success, setSuccess] = useState<string | null>(null);

  async function handleSubmit(data: RegisterDto) {
    setSuccess(null);
    try {
      const res = await register(data);
      const message =
        typeof res === 'string' ? res : 'Conta criada. Confirme o endereço de e-mail antes de iniciar sessão.';
      startTransition(() => {
        setSuccess(message);
      });
      toastSuccess('Registo concluído', message);
    } catch (err) {
      notifyApiError(err, 'Criação de conta');
    }
  }

  return (
    <>
      {success ? (
        <Alert variant="success" title="Sucesso">
          {success}
        </Alert>
      ) : null}

      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <div style={REGISTER_FIELDS_GRID_STYLE}>
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
