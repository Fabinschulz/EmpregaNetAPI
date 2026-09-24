'use client';

import {
  actionIcons,
  AutocompleteField,
  Button,
  FilterBar,
  FilterField,
  SelectField,
  type AutocompleteOption,
  type SelectOption
} from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { useFilterFormSync } from '@/shared/hooks';
import { DATE_ORDER_BY_OPTIONS, LIST_SEARCH_MAX_LENGTH } from '@/shared/schema';
import { defaultJobsFilter, type JobsFilterFormValues } from './jobs-filter-schema';

const STATUS_OPTIONS = [
  { label: 'Todas', value: 'all' },
  { label: 'Ativas', value: 'active' },
  { label: 'Encerradas', value: 'closed' }
];

const ORDER_BY_OPTIONS: SelectOption[] = [...DATE_ORDER_BY_OPTIONS];

type JobsFilterFieldsProps = {
  onChange: (values: JobsFilterFormValues) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function JobsFilterFields({ onChange, searchOptions, searchLoading }: JobsFilterFieldsProps) {
  const { watch, reset } = useFormContext<JobsFilterFormValues>();

  const search = watch('search');
  const status = watch('status');
  const orderBy = watch('orderBy');

  useFilterFormSync({ search, status, orderBy }, onChange);

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultJobsFilter)}
        >
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
          maxLength={LIST_SEARCH_MAX_LENGTH}
        />
      </FilterField>
      <SelectField name="status" label="Situação" options={STATUS_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
