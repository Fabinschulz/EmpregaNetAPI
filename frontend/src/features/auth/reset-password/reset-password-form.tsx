'use client';

import {
  actionIcons,
  FormSubmitButton,
  InputField,
  StandalonePageFormActions,
  StandalonePageFormGrid
} from '@/shared/components';

export function ResetPasswordFormFields() {
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
        <FormSubmitButton variant="primary" icon={actionIcons.password}>
          Redefinir senha
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
