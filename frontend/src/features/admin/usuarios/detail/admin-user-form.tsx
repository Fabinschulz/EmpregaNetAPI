'use client';

import { actionIcons, FormActions, FormGrid, FormSubmitButton, SelectField } from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { USER_TYPE_OPTIONS } from '@/shared/utils';

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
        <FormSubmitButton variant="primary" icon={actionIcons.save} disabled={readOnly}>
          Salvar
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
