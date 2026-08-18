'use client';

import { AutocompleteField, Button, FilterBar, FilterField, SelectField, type AutocompleteOption } from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { useFilterFormSync } from '@/shared/hooks';
import { defaultJobsFilter, jobsFilterToParams, type JobsFilterFormValues } from './jobs-filter-schema';
import type { JobsListQueryParams } from '@/shared/schema';
import { X } from 'lucide-react';

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

  useFilterFormSync(jobsFilterToParams({ search, status }), onChange);

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
