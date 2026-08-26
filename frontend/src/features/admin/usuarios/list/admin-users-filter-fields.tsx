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
  adminUsersFilterToParams,
  defaultAdminUsersFilter,
  type AdminUsersFilterFormValues
} from './admin-users-filter-schema';
import { LIST_ORDER_BY_OPTIONS, type AdminUsersListQueryParams } from '@/shared/schema';

const SITUATION_OPTIONS = [
  { label: 'Todos', value: 'all' },
  { label: 'Ativos', value: 'active' },
  { label: 'Excluídos', value: 'deleted' }
];

type AdminUsersFilterParams = Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

type AdminUsersFilterFieldsProps = {
  onChange: (params: AdminUsersFilterParams) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function AdminUsersFilterFields({ onChange, searchOptions, searchLoading }: AdminUsersFilterFieldsProps) {
  const { watch, reset } = useFormContext<AdminUsersFilterFormValues>();

  const search = watch('search');
  const situation = watch('situation');
  const orderBy = watch('orderBy');

  useFilterFormSync(adminUsersFilterToParams({ search, situation, orderBy }), onChange);

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultAdminUsersFilter)}
        >
          Limpar
        </Button>
      }
    >
      <FilterField span={2}>
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Nome, e-mail ou CPF"
          options={searchOptions}
          loading={searchLoading}
        />
      </FilterField>
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={LIST_ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
