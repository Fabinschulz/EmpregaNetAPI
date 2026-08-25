'use client';

import type { JobsFeedFilters } from '../filters/jobs-feed-filters';
import {
  experienceLevelVocabulary,
  findSalaryRange,
  jobAreaVocabulary,
  jobTypeVocabulary,
  publishedWithinLabel,
  ufFullLabel,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { actionIcons } from '@/shared/components';
import styles from './active-chips.module.scss';
import type { JobsFeedFiltersController } from '../filters';

type FeedActiveChipsProps = {
  controller: JobsFeedFiltersController;
};

/** Chaves cujo valor é uma lista de strings. */
type StringArrayFilterKey = {
  [K in keyof JobsFeedFilters]: JobsFeedFilters[K] extends readonly string[] ? K : never;
}[keyof JobsFeedFilters];

type ActiveChip = {
  key: string;
  label: string;
  onRemove: () => void;
};

export function FeedActiveChips({ controller }: FeedActiveChipsProps) {
  const { filters, toggleFilterValue, updateFilters, clearAll, activeCount } = controller;

  if (activeCount === 0) return null;

  const chips: ActiveChip[] = [];

  const addMany = <K extends StringArrayFilterKey>(key: K, label: (value: string) => string) => {
    (filters[key] as readonly JobsFeedFilters[K][number][]).forEach((value) => {
      chips.push({
        key: `${String(key)}:${value}`,
        label: label(value),
        onRemove: () => toggleFilterValue(key, value)
      });
    });
  };

  if (filters.search.trim()) {
    chips.push({
      key: 'search',
      label: `"${filters.search.trim()}"`,
      onRemove: () => updateFilters({ search: '' })
    });
  }

  if (filters.onlyPcd) {
    chips.push({
      key: 'pcd',
      label: 'Vagas para PcD',
      onRemove: () => updateFilters({ onlyPcd: false })
    });
  }

  addMany('workShifts', workShiftVocabulary.label);
  addMany('experienceLevels', experienceLevelVocabulary.label);
  addMany('areas', jobAreaVocabulary.label);
  addMany('jobTypes', jobTypeVocabulary.label);
  addMany('workModels', workModelVocabulary.label);
  addMany('states', ufFullLabel);
  addMany('cities', (city) => city);
  addMany('requirements', (requirement) => requirement);
  addMany('benefits', (benefit) => benefit);

  const salaryRange = findSalaryRange(filters.salaryRange);
  if (salaryRange) {
    chips.push({
      key: 'salary',
      label: salaryRange.label,
      onRemove: () => updateFilters({ salaryRange: null })
    });
  }

  if (filters.publishedWithin) {
    chips.push({
      key: 'publishedWithin',
      label: publishedWithinLabel(filters.publishedWithin),
      onRemove: () => updateFilters({ publishedWithin: null })
    });
  }

  return (
    <div className={styles.activeChips}>
      <ul className={styles.activeChipsList} aria-label="Filtros aplicados">
        {chips.map((chip) => (
          <li key={chip.key}>
            <button
              type="button"
              className={styles.activeChip}
              onClick={chip.onRemove}
              aria-label={`Remover filtro ${chip.label}`}
            >
              {chip.label}
              <actionIcons.close className={styles.activeChipIcon} aria-hidden />
            </button>
          </li>
        ))}
      </ul>

      <button type="button" className={styles.clearAll} onClick={clearAll}>
        <actionIcons.clearFilters className={styles.clearAllIcon} aria-hidden />
        <span className={styles.clearAllLabel}>Limpar tudo</span>
      </button>
    </div>
  );
}
