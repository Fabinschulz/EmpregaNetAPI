'use client';

import { AutocompleteField, Button, FilterBar, FilterField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import { defaultJobsSearch, type JobsSearchFormValues } from '@/services';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

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
    <FilterBar
      actions={
        <Button type="button" variant="outline" onClick={() => reset(defaultJobsSearch)}>
          <X aria-hidden />
          Limpar
        </Button>
      }
    >
      <FilterField span="full">
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Cargo, palavra-chave..."
          options={searchOptions}
          loading={searchLoading}
        />
      </FilterField>
    </FilterBar>
  );
}
