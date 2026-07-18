'use client';

import { Button, AutocompleteField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import { defaultJobsSearch, type JobsSearchFormValues } from '@/services';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';
import styles from './jobs-search-fields.module.scss';

type JobsSearchFieldsProps = {
  onChange: (search: string | undefined) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function JobsSearchFields({ onChange, searchOptions, searchLoading }: JobsSearchFieldsProps) {
  const { watch, reset } = useFormContext<JobsSearchFormValues>();
  const search = watch('search');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(search.trim() || undefined);
  }, [search, onChange]);

  return (
    <div className={styles.fields}>
      <AutocompleteField
        name="search"
        label="Buscar"
        placeholder="Cargo, palavra-chave..."
        className={styles.search}
        options={searchOptions}
        loading={searchLoading}
      />
      <div className={styles.actions}>
        <Button type="button" variant="outline" onClick={() => reset(defaultJobsSearch)}>
          <X aria-hidden />
          Limpar
        </Button>
      </div>
    </div>
  );
}
