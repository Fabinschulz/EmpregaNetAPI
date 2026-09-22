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
  MAX_JOB_POSITIONS,
  MIN_JOB_POSITIONS,
  UF_SELECT_OPTIONS,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { useMemo } from 'react';
import { useSelectableCompaniesQuery } from '../service';
import { PCD_OPTIONS, SALARY_DISCLOSURE_OPTIONS, type JobFormValues } from './job-form-schema';

type JobFormFieldsProps = {
  /**
   * Posições já ocupadas por candidatos aprovados. Só existe na edição, e é o que impede o
   * recrutador de baixar o total para um número que a API vai recusar.
   */
  filledPositions?: number;

  /**
   * Vaga encerrada. O total deixa de ser editável: subi-lo criaria uma vaga encerrada com posição
   * livre, e a API recusa - o campo desabilitado diz isso antes de o recrutador digitar.
   */
  isClosed?: boolean;
};

export function JobFormFields({ filledPositions, isClosed = false }: JobFormFieldsProps = {}) {
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

  const autoCloseHint = 'A vaga encerra automaticamente quando todas as posições forem preenchidas.';
  const positionsHint = isClosed
    ? 'A vaga está encerrada: o total de vagas não pode mais ser alterado.'
    : filledPositions && filledPositions > 0
      ? `${filledPositions} ${filledPositions === 1 ? 'vaga já preenchida' : 'vagas já preenchidas'}: ` +
        `o total não pode ficar abaixo desse número. ${autoCloseHint}`
      : autoCloseHint;

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

      <FormSection title="Quantidade de vagas" cols={3}>
        <InputField
          name="positions"
          label="Total de vagas"
          type="number"
          min={MIN_JOB_POSITIONS}
          max={MAX_JOB_POSITIONS}
          step="1"
          required
          disabled={isClosed}
          hint={positionsHint}
        />
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
