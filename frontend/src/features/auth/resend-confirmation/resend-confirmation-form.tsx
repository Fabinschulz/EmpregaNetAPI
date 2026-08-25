'use client';

import {
  actionIcons,
  FormSubmitButton,
  InputField,
  StandalonePageFormActions,
  StandalonePageFormGrid
} from '@/shared/components';

export function ResendConfirmationFormFields() {
  return (
    <StandalonePageFormGrid>
      <InputField name="email" label="E-mail da conta" type="email" autoComplete="email" required />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary" icon={actionIcons.send}>
          Reenviar confirmação
        </FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
