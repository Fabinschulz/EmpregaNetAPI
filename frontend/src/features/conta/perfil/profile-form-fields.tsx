'use client';

import {
  actionIcons,
  FormActions,
  FormGrid,
  FormRow,
  FormSubmitButton,
  InputField,
  PhoneField
} from '@/shared/components';

export function ProfileFormFields() {
  return (
    <FormGrid narrow>
      <FormRow>
        <InputField name="username" label="Nome de usuário" required />
        <InputField name="email" label="E-mail" type="email" required />
      </FormRow>
      <PhoneField name="phoneNumber" label="Telefone" />
      <FormActions>
        <FormSubmitButton variant="primary" icon={actionIcons.save}>
          Salvar alterações
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
