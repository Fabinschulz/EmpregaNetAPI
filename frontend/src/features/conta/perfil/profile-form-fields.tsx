'use client';

import { FormActions, FormGrid, FormRow, FormSubmitButton, InputField, PhoneField } from '@/components';
import { Save } from 'lucide-react';

export function ProfileFormFields() {
  return (
    <FormGrid>
      <FormRow>
        <InputField name="username" label="Nome de usuário" required />
        <InputField name="email" label="E-mail" type="email" required />
      </FormRow>
      <PhoneField name="phoneNumber" label="Telefone" />
      <FormActions>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          Salvar alterações
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
