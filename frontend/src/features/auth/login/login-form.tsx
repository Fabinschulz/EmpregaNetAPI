'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { AuthFormActions, AuthFormGrid } from '../shared';

export function LoginFormFields() {
  const { submitting } = useFormContext();

  return (
    <AuthFormGrid>
      <InputField name="login" label="E-mail" type="email" autoComplete="email" required />
      <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
      <AuthFormActions>
        <FormSubmitButton variant="primary" size="lg">
          {submitting ? 'Entrando...' : 'Entrar'}
        </FormSubmitButton>
      </AuthFormActions>
    </AuthFormGrid>
  );
}
