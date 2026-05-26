'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { AuthFormActions, AuthFormGrid } from '../shared';

export function ResetPasswordFormFields() {
  const { submitting } = useFormContext();

  return (
    <AuthFormGrid>
      <InputField name="newPassword" label="Nova senha" type="password" autoComplete="new-password" required />
      <InputField
        name="newPasswordConfirmation"
        label="Confirmar nova senha"
        type="password"
        autoComplete="new-password"
        required
      />
      <AuthFormActions>
        <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Redefinir senha'}</FormSubmitButton>
      </AuthFormActions>
    </AuthFormGrid>
  );
}
