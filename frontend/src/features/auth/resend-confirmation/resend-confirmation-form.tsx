'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { AuthFormActions, AuthFormGrid } from '../shared';

export function ResendConfirmationFormFields() {
  const { submitting } = useFormContext();

  return (
    <AuthFormGrid>
      <InputField name="email" label="E-mail da conta" type="email" autoComplete="email" required />
      <AuthFormActions>
        <FormSubmitButton variant="primary">{submitting ? 'Enviando...' : 'Reenviar confirmação'}</FormSubmitButton>
      </AuthFormActions>
    </AuthFormGrid>
  );
}
