'use client';

import { SelectInput } from '@/shared/components';
import { JOB_SORT_OPTIONS, type JobSortValue } from '@/shared/schema';
import { useMemo } from 'react';
import styles from './sort-select.module.scss';

type FeedSortSelectProps = {
  value: JobSortValue;
  onChange: (value: JobSortValue) => void;
  hasSearch: boolean;
};

export function FeedSortSelect({ value, onChange, hasSearch }: FeedSortSelectProps) {
  /** Relevância sem termo de busca não ordena nada: fica visível, mas bloqueada. */
  const options = useMemo(
    () => JOB_SORT_OPTIONS.map((option) => ({ ...option, disabled: option.value === 'Relevance' && !hasSearch })),
    [hasSearch]
  );

  return (
    <div className={styles.sort}>
      <label className={styles.sortLabel} htmlFor="jobs-feed-sort">
        Ordenar por
      </label>

      <SelectInput
        value={value}
        onChange={(next) => onChange(next as JobSortValue)}
        options={options}
        className={styles.sortTrigger}
        triggerProps={{ id: 'jobs-feed-sort', 'aria-label': 'Ordenar vagas por' }}
      />
    </div>
  );
}
