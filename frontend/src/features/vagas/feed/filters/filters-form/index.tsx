'use client';

import { CheckboxGroup, GroupedCheckboxes, RadioGroup } from '@/components';
import type { JobVocabularyDto, JobsFeedFilters } from '@/features/vagas/service';
import {
  PUBLISHED_WITHIN_OPTIONS,
  SALARY_RANGE_OPTIONS,
  experienceLevelVocabulary,
  findSalaryRange,
  jobAreaVocabulary,
  jobTypeVocabulary,
  publishedWithinLabel,
  workModelVocabulary,
  workShiftVocabulary,
  type PublishedWithinValue
} from '@/shared/schema';
import { Accessibility } from 'lucide-react';
import { FilterSection } from '../filter-section';
import styles from '../filters.module.scss';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';

type FeedFiltersFormProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyDto;
};

type ArrayFilterKey = {
  [K in keyof JobsFeedFilters]: JobsFeedFilters[K] extends readonly string[] ? K : never;
}[keyof JobsFeedFilters];

const SEARCH_AFTER = 10;

function listSummary(values: readonly string[], toLabel: (value: string) => string) {
  return values.length === 0 ? undefined : values.map(toLabel).join(', ');
}

export function FeedFiltersForm({ controller, vocabulary }: FeedFiltersFormProps) {
  const { filters, toggleFilterValue, updateFilters } = controller;

  const toggle =
    <K extends ArrayFilterKey>(key: K) =>
    (value: JobsFeedFilters[K][number]) =>
      toggleFilterValue(key, value);

  return (
    <div className={styles.form}>
      <label className={styles.pcdRow}>
        <input
          type="checkbox"
          className={styles.pcdControl}
          checked={filters.onlyPcd}
          onChange={() => updateFilters({ onlyPcd: !filters.onlyPcd })}
        />

        <Accessibility className={styles.pcdIcon} aria-hidden />

        <span className={styles.pcdText}>
          <span className={styles.pcdTitle}>Somente vagas para PcD</span>
          <span className={styles.pcdHint}>Vagas reservadas a pessoas com deficiência</span>
        </span>
      </label>

      <FilterSection
        title="Área"
        defaultOpen
        activeCount={filters.areas.length}
        summary={listSummary(filters.areas, jobAreaVocabulary.label)}
      >
        <CheckboxGroup
          legend="Área"
          legendHidden
          columns
          searchAfter={SEARCH_AFTER}
          options={jobAreaVocabulary.options}
          selected={filters.areas}
          onToggle={toggle('areas')}
        />
      </FilterSection>

      <FilterSection
        title="Modalidade"
        activeCount={filters.workModels.length}
        summary={listSummary(filters.workModels, workModelVocabulary.label)}
      >
        <CheckboxGroup
          legend="Modalidade"
          legendHidden
          columns
          options={workModelVocabulary.options}
          selected={filters.workModels}
          onToggle={toggle('workModels')}
        />
      </FilterSection>

      <FilterSection
        title="Tipo de contratação"
        activeCount={filters.jobTypes.length}
        summary={listSummary(filters.jobTypes, jobTypeVocabulary.label)}
      >
        <CheckboxGroup
          legend="Tipo de contratação"
          legendHidden
          columns
          options={jobTypeVocabulary.options}
          selected={filters.jobTypes}
          onToggle={toggle('jobTypes')}
        />
      </FilterSection>

      <FilterSection
        title="Faixa salarial"
        activeCount={filters.salaryRange ? 1 : 0}
        summary={findSalaryRange(filters.salaryRange)?.label}
      >
        <RadioGroup
          legend="Faixa salarial"
          legendHidden
          columns
          name="salary-range"
          options={SALARY_RANGE_OPTIONS}
          selected={filters.salaryRange}
          onSelect={(value) => updateFilters({ salaryRange: value })}
        />
      </FilterSection>

      <FilterSection
        title="Experiência exigida"
        activeCount={filters.experienceLevels.length}
        summary={listSummary(filters.experienceLevels, experienceLevelVocabulary.label)}
      >
        <CheckboxGroup
          legend="Experiência exigida"
          legendHidden
          columns
          options={experienceLevelVocabulary.options}
          selected={filters.experienceLevels}
          onToggle={toggle('experienceLevels')}
        />
      </FilterSection>

      {/* <FilterSection
        title="Estado"
        activeCount={filters.states.length}
        summary={listSummary(filters.states, ufFullLabel)}
      >
        <CheckboxGroup
          legend="Estado"
          legendHidden
          columns
          searchAfter={SEARCH_AFTER}
          options={UF_SELECT_OPTIONS}
          selected={filters.states}
          onToggle={toggle('states')}
        />
      </FilterSection> */}

      <FilterSection
        title="Turno"
        activeCount={filters.workShifts.length}
        summary={listSummary(filters.workShifts, workShiftVocabulary.label)}
      >
        <CheckboxGroup
          legend="Turno"
          legendHidden
          columns
          options={workShiftVocabulary.options}
          selected={filters.workShifts}
          onToggle={toggle('workShifts')}
        />
      </FilterSection>

      <FilterSection
        title="Benefícios"
        activeCount={filters.benefits.length}
        summary={listSummary(filters.benefits, (benefit) => benefit)}
      >
        <GroupedCheckboxes
          legend="Benefícios"
          legendHidden
          columns
          searchAfter={SEARCH_AFTER}
          groups={vocabulary.benefits}
          selected={filters.benefits}
          onToggle={toggle('benefits')}
        />
      </FilterSection>

      <FilterSection
        title="Requisitos"
        activeCount={filters.requirements.length}
        summary={listSummary(filters.requirements, (requirement) => requirement)}
      >
        <GroupedCheckboxes
          legend="Requisitos"
          legendHidden
          columns
          searchAfter={SEARCH_AFTER}
          groups={vocabulary.requirements}
          selected={filters.requirements}
          onToggle={toggle('requirements')}
        />
      </FilterSection>

      <FilterSection
        title="Data da publicação"
        activeCount={filters.publishedWithin ? 1 : 0}
        summary={filters.publishedWithin ? publishedWithinLabel(filters.publishedWithin) : undefined}
      >
        <RadioGroup
          legend="Data da publicação"
          legendHidden
          columns
          name="published-within"
          options={PUBLISHED_WITHIN_OPTIONS}
          selected={filters.publishedWithin}
          onSelect={(value) => updateFilters({ publishedWithin: (value as PublishedWithinValue | null) ?? null })}
        />
      </FilterSection>
    </div>
  );
}
