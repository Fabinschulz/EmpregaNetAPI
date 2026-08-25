'use client';

import {
  actionIcons,
  FormSubmitButton,
  InputField,
  StandalonePageFormActions,
  StandalonePageFormGrid
} from '@/shared/components';

export function LoginFormFields() {
  return (
    <StandalonePageFormGrid>
      <InputField name="login" label="E-mail" type="email" autoComplete="email" required />
      <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary" size="lg" icon={actionIcons.signIn}>
          Entrar
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
