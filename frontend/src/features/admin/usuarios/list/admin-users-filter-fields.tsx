'use client';

import { AutocompleteField, Button, FilterBar, FilterField, SelectField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import { adminUsersFilterToParams, defaultAdminUsersFilter, type AdminUsersFilterFormValues } from '../service';
import { LIST_ORDER_BY_OPTIONS, type AdminUsersListQueryParams } from '@/shared';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

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

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(adminUsersFilterToParams({ search, situation, orderBy }));
  }, [search, situation, orderBy, onChange]);

  return (
    <FilterBar
      actions={
        <Button type="button" variant="outline" onClick={() => reset(defaultAdminUsersFilter)}>
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
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={[...LIST_ORDER_BY_OPTIONS]} />
    </FilterBar>
  );
}
