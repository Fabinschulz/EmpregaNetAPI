'use client';

import { Button, AutocompleteField, SelectField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import {
    adminUsersFilterToParams,
    defaultAdminUsersFilter,
    type AdminUsersFilterFormValues
} from '@/services';
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
    <>
      <AutocompleteField
        name="search"
        label="Buscar"
        placeholder="Nome ou e-mail"
        options={searchOptions}
        loading={searchLoading}
      />
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={[...LIST_ORDER_BY_OPTIONS]} />
      <div style={{ display: 'flex', gap: 8, flex: '0 0 auto' }}>
        <Button type="button" variant="outline" onClick={() => reset(defaultAdminUsersFilter)}>
          <X aria-hidden />
          Limpar
        </Button>
      </div>
    </>
  );
}
