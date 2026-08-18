'use client';

import {
  AutocompleteField,
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
import { X } from 'lucide-react';

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
        <Button type="button" variant="outline" onClick={() => reset(defaultCandidatesFilter)}>
          <X aria-hidden />
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
