'use client';

import {
  AutocompleteField,
  actionIcons,
  Button,
  FilterBar,
  FilterField,
  SelectField,
  type AutocompleteOption
} from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { useFilterFormSync } from '@/shared/hooks';
import {
  candidatesFilterToParams,
  defaultCandidatesFilter,
  type CandidatesFilterFormValues
} from './candidates-filter-schema';
import { LIST_ORDER_BY_OPTIONS, type CandidatesListQueryParams } from '@/shared/schema';

type CandidatesFilterParams = Pick<CandidatesListQueryParams, 'search' | 'orderBy'>;

type CandidatesFilterFieldsProps = {
  onChange: (params: CandidatesFilterParams) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function CandidatesFilterFields({ onChange, searchOptions, searchLoading }: CandidatesFilterFieldsProps) {
  const { watch, reset } = useFormContext<CandidatesFilterFormValues>();

  const search = watch('search');
  const orderBy = watch('orderBy');

  useFilterFormSync(candidatesFilterToParams({ search, orderBy }), onChange);

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultCandidatesFilter)}
        >
          Limpar
        </Button>
      }
    >
      <FilterField span={2}>
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Nome ou e-mail"
          options={searchOptions}
          loading={searchLoading}
        />
      </FilterField>
      <SelectField name="orderBy" label="Ordenar por" options={LIST_ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
