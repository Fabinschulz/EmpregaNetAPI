'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';

const ADMIN_USER_FORM_GRID_STYLE = {
  display: 'grid',
  gap: 12,
  maxWidth: 520,
  marginTop: 12
} as const;

export function AdminUserFormFields() {
  const { submitting } = useFormContext();

  return (
    <div style={ADMIN_USER_FORM_GRID_STYLE}>
      <InputField
        name="userType"
        label="Tipo de Usuário (ex.: Admin, Recruiter, Manager, Candidate)"
        hint="O backend valida/normaliza; aqui enviamos o valor diretamente."
      />
      <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
        <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Salvar'}</FormSubmitButton>
      </div>
    </div>
  );
}
