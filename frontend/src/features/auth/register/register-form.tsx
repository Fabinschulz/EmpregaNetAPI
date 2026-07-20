'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { StandalonePageFormActions, StandalonePageFormGrid } from '@/components';

export function RegisterFormFields() {
  const { submitting } = useFormContext();

  return (
    <StandalonePageFormGrid>
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
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary">{submitting ? 'Criando conta...' : 'Criar conta'}</FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
