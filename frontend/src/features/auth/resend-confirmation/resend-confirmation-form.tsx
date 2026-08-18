'use client';

import { FormSubmitButton, InputField, StandalonePageFormActions, StandalonePageFormGrid } from '@/shared/components';
import { useFormContext } from '@/shared/context';

export function ResendConfirmationFormFields() {
  const { submitting } = useFormContext();

  return (
    <StandalonePageFormGrid>
      <InputField name="email" label="E-mail da conta" type="email" autoComplete="email" required />
      <StandalonePageFormActions>
        <FormSubmitButton variant="primary">{submitting ? 'Enviando...' : 'Reenviar confirmação'}</FormSubmitButton>
      </StandalonePageFormActions>
    </StandalonePageFormGrid>
  );
}
