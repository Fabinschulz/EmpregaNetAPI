'use client';

import { FormActions, FormGrid, FormSubmitButton, SelectField } from '@/components';
import { useFormContext } from '@/context';
import { USER_TYPE_OPTIONS } from '@/shared';
import { Save } from 'lucide-react';

type AdminUserFormFieldsProps = {
  backHref: string;
};

export function AdminUserFormFields({ backHref }: AdminUserFormFieldsProps) {
  const { submitting, readOnly } = useFormContext();

  return (
    <FormGrid>
      <SelectField
        name="userType"
        label="Tipo de Usuário"
        options={[...USER_TYPE_OPTIONS]}
        placeholder="Selecione o tipo de usuário"
      />
      <FormActions backHref={backHref}>
        <FormSubmitButton variant="primary" disabled={readOnly}>
          <Save aria-hidden />
          {submitting ? 'Salvando...' : 'Salvar'}
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
