'use client';

import { FormSubmitButton, InputField, StandalonePageFormActions, StandalonePageFormGrid } from '@/shared/components';
import { useFormContext } from '@/shared/context';

export function LoginFormFields() {
  const { submitting } = useFormContext();

  return (
    <StandalonePageFormGrid>
      <InputField name="login" label="E-mail" type="email" autoComplete="email" required />
      <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary" size="lg">
          {submitting ? 'Entrando...' : 'Entrar'}
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
