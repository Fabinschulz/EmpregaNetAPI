'use client';

import { Button, FormActions, FormGrid, FormRow, FormSubmitButton, InputField, SelectField, TextareaField } from '@/components';
import { useFormContext } from '@/context';
import { JOB_TYPE_OPTIONS, useSelectableCompaniesQuery } from './service';
import { Archive, Save } from 'lucide-react';
import { useMemo } from 'react';

const JOB_TYPE_SELECT_OPTIONS = JOB_TYPE_OPTIONS.map((o) => ({ value: o.value, label: o.label }));

type JobFormFieldsProps = {
  submitLabel: string;
  onClose?: () => void;
  closeDisabled?: boolean;
};

export function JobFormFields({ submitLabel, onClose, closeDisabled }: JobFormFieldsProps) {
  const { submitting } = useFormContext();
  const { data: companies, isPending: companiesLoading } = useSelectableCompaniesQuery();

  const companyOptions = useMemo(
    () => (companies ?? []).map((company) => ({ value: String(company.id), label: company.name })),
    [companies]
  );

  return (
    <FormGrid>
      <SelectField
        name="companyId"
        label="Empresa"
        options={companyOptions}
        placeholder={companiesLoading ? 'Carregando empresas...' : 'Selecione a empresa'}
        required
      />
      <InputField name="title" label="Título" required />
      <FormRow>
        <SelectField name="jobType" label="Tipo de vaga" options={JOB_TYPE_SELECT_OPTIONS} required />
        <InputField name="salary" label="Salário (R$)" type="number" min={0} step="0.01" required />
      </FormRow>
      <TextareaField name="description" label="Descrição" rows={5} />
      <FormActions>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          {submitting ? 'Salvando...' : submitLabel}
        </FormSubmitButton>
        {onClose ? (
          <Button type="button" onClick={onClose} disabled={closeDisabled}>
            <Archive aria-hidden />
            Encerrar vaga
          </Button>
        ) : null}
      </FormActions>
    </FormGrid>
  );
}
