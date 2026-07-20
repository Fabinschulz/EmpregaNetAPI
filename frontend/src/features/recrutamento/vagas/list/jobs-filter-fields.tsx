'use client';

import { AutocompleteField, Button, FilterBar, FilterField, SelectField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import { defaultJobsFilter, jobsFilterToParams, type JobsFilterFormValues } from '../service';
import type { JobsListQueryParams } from '@/shared/schema';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

const STATUS_OPTIONS = [
  { label: 'Todas', value: 'all' },
  { label: 'Ativas', value: 'active' },
  { label: 'Encerradas', value: 'closed' }
];

type JobsFilterParams = Pick<JobsListQueryParams, 'search' | 'isActive'>;

type JobsFilterFieldsProps = {
  onChange: (params: JobsFilterParams) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function JobsFilterFields({ onChange, searchOptions, searchLoading }: JobsFilterFieldsProps) {
  const { watch, reset } = useFormContext<JobsFilterFormValues>();

  const search = watch('search');
  const status = watch('status');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(jobsFilterToParams({ search, status }));
  }, [search, status, onChange]);

  return (
    <FilterBar
      actions={
        <Button type="button" variant="outline" onClick={() => reset(defaultJobsFilter)}>
          <X aria-hidden />
          Limpar
        </Button>
      }
    >
      <FilterField span={2}>
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Título ou descrição da vaga"
          options={searchOptions}
          loading={searchLoading}
        />
      </FilterField>
      <SelectField name="status" label="Situação" options={STATUS_OPTIONS} />
    </FilterBar>
  );
}
