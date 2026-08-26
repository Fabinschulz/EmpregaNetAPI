'use client';

import {
  actionIcons,
  CpfField,
  FormSubmitButton,
  InputField,
  StandalonePageFormActions,
  StandalonePageFormGrid
} from '@/shared/components';

export function RegisterFormFields() {
  return (
    <StandalonePageFormGrid>
      <InputField name="username" label="Nome de usuário" autoComplete="username" required />
      <InputField name="email" label="E-mail" type="email" autoComplete="email" required />
      <CpfField name="cpf" label="CPF" required hint="Também serve para entrar na sua conta." />
      <InputField name="password" label="Senha" type="password" autoComplete="new-password" required />
      <InputField
        name="passwordConfirmation"
        label="Confirmar senha"
        type="password"
        autoComplete="new-password"
        required
      />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary" icon={actionIcons.signUp}>
          Criar conta
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
