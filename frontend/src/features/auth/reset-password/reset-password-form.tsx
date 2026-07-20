'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { StandalonePageFormActions, StandalonePageFormGrid } from '@/components';

export function ResetPasswordFormFields() {
  const { submitting } = useFormContext();

  return (
    <StandalonePageFormGrid>
      <InputField name="newPassword" label="Nova senha" type="password" autoComplete="new-password" required />
      <InputField
        name="newPasswordConfirmation"
        label="Confirmar nova senha"
        type="password"
        autoComplete="new-password"
        required
      />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Redefinir senha'}</FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
