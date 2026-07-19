'use client';

import { FormActions, FormGrid, FormRow, FormSubmitButton, InputField } from '@/components';
import { KeyRound } from 'lucide-react';

export function ChangePasswordFormFields() {
  return (
    <FormGrid>
      <InputField name="currentPassword" label="Senha atual" type="password" required />
      <FormRow>
        <InputField name="newPassword" label="Nova senha" type="password" required />
        <InputField name="newPasswordConfirmation" label="Confirmar nova senha" type="password" required />
      </FormRow>
      <FormActions>
        <FormSubmitButton variant="primary">
          <KeyRound aria-hidden />
          Alterar senha
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
