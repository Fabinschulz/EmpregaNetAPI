'use client';

import { FormCol, FormGrid, FormSection, InputField, PhoneField, SelectField } from '@/shared/components';
import { useZipCodeAutofill } from '@/shared/hooks';
import { UF_OPTIONS } from '@/shared/schema';
import { TYPE_OF_ACTIVITY_OPTIONS } from '../domain/type-of-activity';

const ACTIVITY_OPTIONS = TYPE_OF_ACTIVITY_OPTIONS.map((o) => ({ value: o.value, label: o.label }));
const STATE_OPTIONS = UF_OPTIONS.map((o) => ({ value: o.value, label: `${o.value} - ${o.label}` }));

const ADDRESS_FIELDS = {
  zipCode: 'address.zipCode',
  street: 'address.street',
  neighborhood: 'address.neighborhood',
  city: 'address.city',
  state: 'address.state'
} as const;

export function CompanyFormFields() {
  const { hint: zipCodeHint, onZipCodeChange } = useZipCodeAutofill(ADDRESS_FIELDS);

  return (
    <FormGrid>
      <FormSection title="Informações essenciais" cols={4}>
        <FormCol span={2}>
          <InputField name="companyName" label="Nome" required />
        </FormCol>
        <InputField name="cnpj" label="CNPJ" placeholder="Somente números" required />
        <SelectField name="typeOfActivity" label="Tipo de atividade" options={ACTIVITY_OPTIONS} required />
        <FormCol span={2}>
          <InputField name="email" label="E-mail" type="email" required />
        </FormCol>
        <FormCol span={2}>
          <PhoneField name="phone" label="Telefone" required />
        </FormCol>
      </FormSection>

      <FormSection
        title="Endereço"
        description="Informe o CEP para preencher logradouro, bairro, cidade e estado automaticamente."
        cols={4}
      >
        <InputField
          name="address.zipCode"
          label="CEP"
          placeholder="00000-000"
          inputMode="numeric"
          autoComplete="postal-code"
          maxLength={9}
          hint={zipCodeHint}
          onChange={(event) => void onZipCodeChange(event.target.value)}
          required
        />
        <FormCol span={2}>
          <InputField name="address.street" label="Logradouro" required />
        </FormCol>
        <InputField name="address.number" label="Número" required />
        <InputField name="address.complement" label="Complemento" />
        <InputField name="address.neighborhood" label="Bairro" required />
        <InputField name="address.city" label="Cidade" required />
        <SelectField name="address.state" label="Estado (UF)" options={STATE_OPTIONS} required />
      </FormSection>
    </FormGrid>
  );
}
