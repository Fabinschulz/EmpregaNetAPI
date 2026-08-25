'use client';

import { actionIcons, FormActions, FormGrid, FormRow, FormSubmitButton, InputField } from '@/shared/components';

export function ChangePasswordFormFields() {
  return (
    <FormGrid narrow>
      <InputField name="currentPassword" label="Senha atual" type="password" required />
      <FormRow>
        <InputField name="newPassword" label="Nova senha" type="password" required />
        <InputField name="newPasswordConfirmation" label="Confirmar nova senha" type="password" required />
      </FormRow>
      <FormActions>
        <FormSubmitButton variant="primary" icon={actionIcons.password}>
          Alterar senha
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
