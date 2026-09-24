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
import { LIST_ORDER_BY_OPTIONS, LIST_SEARCH_MAX_LENGTH } from '@/shared/schema';
import { USER_TYPE_OPTIONS } from '@/shared/utils';
import { defaultAdminUsersFilter, type AdminUsersFilterFormValues } from './admin-users-filter-schema';

const SITUATION_OPTIONS = [
  { label: 'Todos', value: 'all' },
  { label: 'Ativos', value: 'active' },
  { label: 'Excluídos', value: 'deleted' }
];

const USER_TYPE_FILTER_OPTIONS: SelectOption[] = [{ label: 'Todos', value: 'all' }, ...USER_TYPE_OPTIONS];

type AdminUsersFilterFieldsProps = {
  onChange: (values: AdminUsersFilterFormValues) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function AdminUsersFilterFields({ onChange, searchOptions, searchLoading }: AdminUsersFilterFieldsProps) {
  const { watch, reset } = useFormContext<AdminUsersFilterFormValues>();

  const search = watch('search');
  const situation = watch('situation');
  const userType = watch('userType');
  const orderBy = watch('orderBy');

  useFilterFormSync({ search, situation, userType, orderBy }, onChange);

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
          maxLength={LIST_SEARCH_MAX_LENGTH}
        />
      </FilterField>
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="userType" label="Tipo de usuário" options={USER_TYPE_FILTER_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={LIST_ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
