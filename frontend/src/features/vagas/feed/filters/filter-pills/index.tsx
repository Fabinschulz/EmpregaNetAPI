'use client';

import { CheckboxGroup, GroupedCheckboxes, RadioGroup } from '@/components';
import type { JobVocabularyDto } from '@/features/vagas/service';
import {
  PUBLISHED_WITHIN_OPTIONS,
  SALARY_RANGE_OPTIONS,
  experienceLevelVocabulary,
  jobAreaVocabulary,
  workShiftVocabulary,
  type PublishedWithinValue
} from '@/shared/schema';
import { FilterPill, TogglePill } from '../filter-pill';
import { FeedFiltersDrawer } from '../filters-drawer';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';
import styles from './filter-pills.module.scss';

type FeedFilterPillsProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyDto;
  totalItems: number;
};

const SCROLL_AFTER = 8;

export function FeedFilterPills({ controller, vocabulary, totalItems }: FeedFilterPillsProps) {
  const { filters, toggleFilterValue, updateFilters } = controller;

  return (
    <div className={styles.bar} role="group" aria-label="Filtros de vagas">
      <FilterPill label="Turno" activeCount={filters.workShifts.length}>
        <CheckboxGroup
          legend="Turno"
          legendHidden
          options={workShiftVocabulary.options}
          selected={filters.workShifts}
          onToggle={(value) => toggleFilterValue('workShifts', value)}
        />
      </FilterPill>

      <FilterPill label="Faixa salarial" activeCount={filters.salaryRange ? 1 : 0}>
        <RadioGroup
          legend="Faixa salarial"
          legendHidden
          name="pill-salary-range"
          options={SALARY_RANGE_OPTIONS}
          selected={filters.salaryRange}
          onSelect={(value) => updateFilters({ salaryRange: value })}
        />
      </FilterPill>

      <FilterPill label="Experiência" activeCount={filters.experienceLevels.length}>
        <CheckboxGroup
          legend="Experiência exigida"
          legendHidden
          options={experienceLevelVocabulary.options}
          selected={filters.experienceLevels}
          onToggle={(value) => toggleFilterValue('experienceLevels', value)}
        />
      </FilterPill>

      <FilterPill label="Área" activeCount={filters.areas.length}>
        <CheckboxGroup
          legend="Área"
          legendHidden
          options={jobAreaVocabulary.options}
          selected={filters.areas}
          onToggle={(value) => toggleFilterValue('areas', value)}
          scrollAfter={SCROLL_AFTER}
        />
      </FilterPill>

      <FilterPill label="Benefícios" activeCount={filters.benefits.length}>
        <GroupedCheckboxes
          legend="Benefícios"
          legendHidden
          groups={vocabulary.benefits}
          selected={filters.benefits}
          onToggle={(value) => toggleFilterValue('benefits', value)}
        />
      </FilterPill>

      <FilterPill label="Publicação" activeCount={filters.publishedWithin ? 1 : 0}>
        <RadioGroup
          legend="Data da publicação"
          legendHidden
          name="pill-published-within"
          options={PUBLISHED_WITHIN_OPTIONS}
          selected={filters.publishedWithin}
          onSelect={(value) => updateFilters({ publishedWithin: (value as PublishedWithinValue | null) ?? null })}
        />
      </FilterPill>

      <TogglePill
        label="Somente PcD"
        pressed={filters.onlyPcd}
        onToggle={() => updateFilters({ onlyPcd: !filters.onlyPcd })}
      />

      <FeedFiltersDrawer controller={controller} vocabulary={vocabulary} totalItems={totalItems} />
    </div>
  );
}
