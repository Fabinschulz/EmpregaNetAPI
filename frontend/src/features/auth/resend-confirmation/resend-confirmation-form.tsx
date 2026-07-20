'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { StandalonePageFormActions, StandalonePageFormGrid } from '@/components';

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
