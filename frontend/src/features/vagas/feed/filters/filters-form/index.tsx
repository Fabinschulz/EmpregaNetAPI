'use client';

import { CheckboxGroup, GroupedCheckboxes, RadioGroup } from '@/components';
import type { JobVocabularyDto, JobsFeedFilters } from '@/features/vagas/service';
import {
  PUBLISHED_WITHIN_OPTIONS,
  SALARY_RANGE_OPTIONS,
  UF_SELECT_OPTIONS,
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  workModelVocabulary,
  workShiftVocabulary,
  type PublishedWithinValue
} from '@/shared/schema';
import styles from '../filters.module.scss';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';

type FeedFiltersFormProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyDto;
};

type ArrayFilterKey = {
  [K in keyof JobsFeedFilters]: JobsFeedFilters[K] extends readonly string[] ? K : never;
}[keyof JobsFeedFilters];

/** Acima de 8 opções a lista rola: o painel inteiro não deve crescer com uma seção só. */
const SCROLL_AFTER = 8;

const PCD_OPTION = [{ value: 'pcd', label: 'Somente vagas para PcD' }] as const;
const PCD_ON = ['pcd'] as const;
const PCD_OFF = [] as const;

export function FeedFiltersForm({ controller, vocabulary }: FeedFiltersFormProps) {
  const { filters, toggleFilterValue, updateFilters } = controller;

  const toggle =
    <K extends ArrayFilterKey>(key: K) =>
    (value: JobsFeedFilters[K][number]) =>
      toggleFilterValue(key, value);

  return (
    <div className={styles.form}>
      <CheckboxGroup
        legend="Acessibilidade"
        options={PCD_OPTION}
        selected={filters.onlyPcd ? PCD_ON : PCD_OFF}
        onToggle={() => updateFilters({ onlyPcd: !filters.onlyPcd })}
      />

      <CheckboxGroup
        legend="Turno"
        options={workShiftVocabulary.options}
        selected={filters.workShifts}
        onToggle={toggle('workShifts')}
      />

      <CheckboxGroup
        legend="Experiência exigida"
        options={experienceLevelVocabulary.options}
        selected={filters.experienceLevels}
        onToggle={toggle('experienceLevels')}
      />

      <RadioGroup
        legend="Faixa salarial"
        name="salary-range"
        options={SALARY_RANGE_OPTIONS}
        selected={filters.salaryRange}
        onSelect={(value) => updateFilters({ salaryRange: value })}
      />

      <CheckboxGroup
        legend="Área"
        options={jobAreaVocabulary.options}
        selected={filters.areas}
        onToggle={toggle('areas')}
        scrollAfter={SCROLL_AFTER}
      />

      <CheckboxGroup
        legend="Tipo de contratação"
        options={jobTypeVocabulary.options}
        selected={filters.jobTypes}
        onToggle={toggle('jobTypes')}
      />

      <RadioGroup
        legend="Data da publicação"
        name="published-within"
        options={PUBLISHED_WITHIN_OPTIONS}
        selected={filters.publishedWithin}
        onSelect={(value) => updateFilters({ publishedWithin: (value as PublishedWithinValue | null) ?? null })}
      />

      <GroupedCheckboxes
        legend="Requisitos"
        groups={vocabulary.requirements}
        selected={filters.requirements}
        onToggle={toggle('requirements')}
      />

      <GroupedCheckboxes
        legend="Benefícios"
        groups={vocabulary.benefits}
        selected={filters.benefits}
        onToggle={toggle('benefits')}
      />

      <CheckboxGroup
        legend="Modalidade"
        options={workModelVocabulary.options}
        selected={filters.workModels}
        onToggle={toggle('workModels')}
      />

      <CheckboxGroup
        legend="Estado"
        options={UF_SELECT_OPTIONS}
        selected={filters.states}
        onToggle={toggle('states')}
        scrollAfter={SCROLL_AFTER}
      />
    </div>
  );
}
