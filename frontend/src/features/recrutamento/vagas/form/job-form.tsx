'use client';

import {
  Button,
  FormActions,
  FormGrid,
  FormRow,
  FormSection,
  FormSubmitButton,
  InputField,
  MultiSelectField,
  SelectField,
  TextareaField
} from '@/components';
import { useFormContext } from '@/context';
import { useJobVocabularyQuery } from '@/features/vagas/service';
import {
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  UF_SELECT_OPTIONS,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { Archive, Save } from 'lucide-react';
import { useMemo } from 'react';
import { useSelectableCompaniesQuery } from '../service';
import { PCD_OPTIONS, SALARY_DISCLOSURE_OPTIONS, type JobFormValues } from './job-form-schema';

type JobFormFieldsProps = {
  submitLabel: string;
  backHref: string;
  onClose?: () => void;
  closeDisabled?: boolean;
};

export function JobFormFields({ submitLabel, backHref, onClose, closeDisabled }: JobFormFieldsProps) {
  const { submitting, watch } = useFormContext<JobFormValues>();
  const { data: companies, isPending: companiesLoading } = useSelectableCompaniesQuery();
  const { data: vocabulary, isPending: vocabularyLoading } = useJobVocabularyQuery();

  const companyOptions = useMemo(
    () => (companies ?? []).map((company) => ({ value: String(company.id), label: company.name })),
    [companies]
  );

  const flatten = (groups: { label: string; items: readonly string[] }[] | undefined) =>
    (groups ?? []).flatMap((group) => group.items.map((item) => ({ value: item, label: `${group.label} · ${item}` })));

  const requirementOptions = useMemo(() => flatten(vocabulary?.requirements), [vocabulary]);
  const benefitOptions = useMemo(() => flatten(vocabulary?.benefits), [vocabulary]);
  const salaryDisclosed = watch('salaryDisclosure') !== 'undisclosed';

  return (
    <FormGrid>
      <FormSection title="Identificação">
        <SelectField
          name="companyId"
          label="Empresa"
          options={companyOptions}
          placeholder="Selecione a empresa"
          loading={companiesLoading}
          required
        />
        <InputField name="title" label="Título da vaga" required />
        <InputField
          name="summary"
          label="Resumo"
          hint="Chamada curta exibida no cartão do feed (até 280 caracteres)."
        />
        <TextareaField name="description" label="Descrição" rows={6} required />
      </FormSection>

      <FormSection title="Jornada e experiência">
        <FormRow>
          <SelectField name="workShift" label="Turno" options={workShiftVocabulary.options} required />
          <SelectField
            name="experienceLevel"
            label="Experiência exigida"
            options={experienceLevelVocabulary.options}
            required
          />
        </FormRow>
        <FormRow>
          <SelectField name="jobType" label="Tipo de contratação" options={jobTypeVocabulary.options} required />
          <SelectField name="area" label="Área" options={jobAreaVocabulary.options} required />
        </FormRow>
        <FormRow>
          <SelectField name="workModel" label="Modalidade" options={workModelVocabulary.options} required />
          <SelectField name="pcd" label="Vaga afirmativa" options={PCD_OPTIONS} required />
        </FormRow>
      </FormSection>

      <FormSection title="Localização">
        <FormRow>
          <InputField name="city" label="Cidade" required />
          <SelectField name="state" label="Estado" options={UF_SELECT_OPTIONS} required />
        </FormRow>
      </FormSection>

      <FormSection title="Remuneração">
        <SelectField name="salaryDisclosure" label="Salário" options={SALARY_DISCLOSURE_OPTIONS} required />
        {salaryDisclosed ? (
          <FormRow>
            <InputField name="salaryMin" label="Salário mínimo (R$)" type="number" min={0} step="0.01" />
            <InputField name="salaryMax" label="Salário máximo (R$)" type="number" min={0} step="0.01" />
          </FormRow>
        ) : null}
      </FormSection>

      <FormSection title="Requisitos e benefícios">
        <MultiSelectField
          name="requirements"
          label="Requisitos"
          options={requirementOptions}
          placeholder="Escolaridade, CNH, NRs, equipamentos..."
          loading={vocabularyLoading}
        />
        <MultiSelectField
          name="benefits"
          label="Benefícios"
          options={benefitOptions}
          placeholder="Fretado, cesta básica, plano de saúde..."
          loading={vocabularyLoading}
        />
      </FormSection>

      <FormActions backHref={backHref}>
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
