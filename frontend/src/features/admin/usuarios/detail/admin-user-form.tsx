'use client';

import { FormActions, FormGrid, FormSubmitButton, SelectField } from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { USER_TYPE_OPTIONS } from '@/shared/utils';
import { Save } from 'lucide-react';

type AdminUserFormFieldsProps = {
  backHref: string;
};

export function AdminUserFormFields({ backHref }: AdminUserFormFieldsProps) {
  const { readOnly } = useFormContext();

  return (
    <FormGrid narrow>
      <SelectField
        name="userType"
        label="Tipo de Usuário"
        options={[...USER_TYPE_OPTIONS]}
        placeholder="Selecione o tipo de usuário"
      />
      <FormActions backHref={backHref}>
        <FormSubmitButton variant="primary" disabled={readOnly}>
          <Save aria-hidden />
          Salvar
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
