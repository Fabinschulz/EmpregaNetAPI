'use client';

import { FormSubmitButton, InputField } from '@/components';
import { useFormContext } from '@/context';
import { Save } from 'lucide-react';
import { CSSProperties } from 'react';

const COMPANY_FORM_GRID_STYLE: CSSProperties = {
  display: 'grid',
  gap: 12,
  maxWidth: 640,
  marginTop: 12
};

type CompanyFormFieldsProps = {
  submitLabel: string;
};

export function CompanyFormFields({ submitLabel }: CompanyFormFieldsProps) {
  const { submitting } = useFormContext();

  return (
    <div style={COMPANY_FORM_GRID_STYLE}>
      <InputField name="name" label="Nome" required />
      <InputField name="email" label="E-mail" type="email" />
      <InputField name="phone" label="Telefone" />
      <InputField name="documentNo" label="Documento" />
      <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          {submitting ? 'Salvando...' : submitLabel}
        </FormSubmitButton>
      </div>
    </div>
  );
}
