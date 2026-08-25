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
import { LIST_ORDER_BY_OPTIONS, type CompaniesListQueryParams } from '@/shared/schema';
import {
  companiesFilterToParams,
  defaultCompaniesFilter,
  type CompaniesFilterFormValues
} from './companies-filter-schema';

const SITUATION_OPTIONS = [
  { label: 'Todas', value: 'all' },
  { label: 'Ativas', value: 'active' },
  { label: 'Excluídas', value: 'deleted' }
];

type CompaniesFilterParams = Pick<CompaniesListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

type CompaniesFilterFieldsProps = {
  onChange: (params: CompaniesFilterParams) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function CompaniesFilterFields({ onChange, searchOptions, searchLoading }: CompaniesFilterFieldsProps) {
  const { watch, reset } = useFormContext<CompaniesFilterFormValues>();

  const [search, situation, orderBy] = watch(['search', 'situation', 'orderBy']);

  useFilterFormSync(companiesFilterToParams({ search, situation, orderBy }), onChange);

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultCompaniesFilter)}
        >
          Limpar
        </Button>
      }
    >
      <FilterField span={2}>
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Nome, e-mail ou CNPJ"
          options={searchOptions}
          loading={searchLoading}
        />
      </FilterField>
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={LIST_ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
