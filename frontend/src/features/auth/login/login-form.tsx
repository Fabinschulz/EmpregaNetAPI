'use client';

import { FormSubmitButton, InputField } from '@/components';
import { CSSProperties } from 'react';
import { useFormContext } from 'src/context';

const LOGIN_FIELDS_GRID_STYLE: CSSProperties = {
  display: 'grid',
  gap: 12,
  marginTop: 14
};

export function LoginFormFields() {
  const { submitting } = useFormContext();

  return (
    <div style={LOGIN_FIELDS_GRID_STYLE}>
      <InputField name="login" label="E-mail" type="email" autoComplete="email" required />
      <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
      <FormSubmitButton variant="primary">{submitting ? 'Entrando...' : 'Entrar'}</FormSubmitButton>
    </div>
  );
}
