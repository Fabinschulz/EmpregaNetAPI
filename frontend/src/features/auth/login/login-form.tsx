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
      <InputField
        name="identifier"
        label="CPF ou e-mail"
        placeholder="Digite seu CPF ou e-mail"
        autoComplete="username"
        autoCapitalize="none"
        spellCheck={false}
        required
      />
      <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary" size="lg" icon={actionIcons.signIn}>
          Entrar
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
