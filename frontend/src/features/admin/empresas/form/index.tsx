'use client';

import {
  FormActions,
  FormGrid,
  FormRow,
  FormSection,
  FormSubmitButton,
  InputField,
  PhoneField,
  SelectField
} from '@/components';
import { useFormContext } from '@/context';
import { TYPE_OF_ACTIVITY_OPTIONS, UF_OPTIONS } from '../service';
import { Save } from 'lucide-react';

const ACTIVITY_OPTIONS = TYPE_OF_ACTIVITY_OPTIONS.map((o) => ({ value: o.value, label: o.label }));
const STATE_OPTIONS = UF_OPTIONS.map((o) => ({ value: o.value, label: `${o.value} - ${o.label}` }));

type CompanyFormFieldsProps = {
  submitLabel: string;
};

export function CompanyFormFields({ submitLabel }: CompanyFormFieldsProps) {
  const { submitting } = useFormContext();

  return (
    <FormGrid>
      <InputField name="companyName" label="Nome" required />
      <FormRow>
        <InputField name="cnpj" label="CNPJ" placeholder="Somente números" required />
        <SelectField name="typeOfActivity" label="Tipo de atividade" options={ACTIVITY_OPTIONS} required />
      </FormRow>
      <FormRow>
        <InputField name="email" label="E-mail" type="email" required />
        <PhoneField name="phone" label="Telefone" required />
      </FormRow>

      <FormSection title="Endereço">
        <FormRow>
          <InputField name="address.zipCode" label="CEP" placeholder="00000-000" required />
          <SelectField name="address.state" label="Estado (UF)" options={STATE_OPTIONS} required />
        </FormRow>
        <InputField name="address.street" label="Logradouro" required />
        <FormRow>
          <InputField name="address.number" label="Número" required />
          <InputField name="address.complement" label="Complemento" />
        </FormRow>
        <FormRow>
          <InputField name="address.neighborhood" label="Bairro" required />
          <InputField name="address.city" label="Cidade" required />
        </FormRow>
      </FormSection>

      <FormActions>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          {submitting ? 'Salvando...' : submitLabel}
        </FormSubmitButton>
      </FormActions>
    </FormGrid>
  );
}
