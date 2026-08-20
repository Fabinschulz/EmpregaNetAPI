'use client';

import { useJobVocabularyQuery } from '@/features/vagas/service';
import {
  FormCol,
  FormGrid,
  FormSection,
  InputField,
  MultiSelectField,
  SelectField,
  TextareaField
} from '@/shared/components';
import { useFormContext } from '@/shared/context';
import {
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  UF_SELECT_OPTIONS,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { useMemo } from 'react';
import { useSelectableCompaniesQuery } from '../service';
import { PCD_OPTIONS, SALARY_DISCLOSURE_OPTIONS, type JobFormValues } from './job-form-schema';

export function JobFormFields() {
  const { watch } = useFormContext<JobFormValues>();
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
      <FormSection title="Identificação" cols={4}>
        <FormCol span={2}>
          <SelectField
            name="companyId"
            label="Empresa"
            options={companyOptions}
            placeholder="Selecione a empresa"
            loading={companiesLoading}
            required
          />
        </FormCol>
        <FormCol span={2}>
          <InputField name="title" label="Título da vaga" required />
        </FormCol>
        <FormCol span="full">
          <InputField
            name="summary"
            label="Resumo"
            hint="Chamada curta exibida no cartão do feed (até 280 caracteres)."
          />
        </FormCol>
        <FormCol span="full">
          <TextareaField name="description" label="Descrição" rows={6} required />
        </FormCol>
      </FormSection>

      <FormSection title="Jornada e experiência" cols={3}>
        <SelectField name="workShift" label="Turno" options={workShiftVocabulary.options} required />
        <SelectField name="jobType" label="Tipo de contratação" options={jobTypeVocabulary.options} required />
        <SelectField name="workModel" label="Modalidade" options={workModelVocabulary.options} required />
        <SelectField name="area" label="Área" options={jobAreaVocabulary.options} required />
        <SelectField
          name="experienceLevel"
          label="Experiência exigida"
          options={experienceLevelVocabulary.options}
          required
        />
        <SelectField name="pcd" label="Vaga afirmativa" options={PCD_OPTIONS} required />
      </FormSection>

      <FormSection title="Localização" cols={3}>
        <FormCol span={2}>
          <InputField name="city" label="Cidade" required />
        </FormCol>
        <SelectField name="state" label="Estado" options={UF_SELECT_OPTIONS} required />
      </FormSection>

      <FormSection title="Remuneração" cols={3}>
        <SelectField name="salaryDisclosure" label="Salário" options={SALARY_DISCLOSURE_OPTIONS} required />
        {salaryDisclosed ? (
          <>
            <InputField name="salaryMin" label="Salário mínimo (R$)" type="number" min={0} step="0.01" />
            <InputField name="salaryMax" label="Salário máximo (R$)" type="number" min={0} step="0.01" />
          </>
        ) : null}
      </FormSection>

      <FormSection title="Requisitos e benefícios" cols={2}>
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
    </FormGrid>
  );
}
