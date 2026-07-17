'use client';

import { Button, InputField, SelectField } from '@/components';
import { useFormContext } from '@/context';
import { useDebouncedValue } from '@/hooks';
import { LIST_ORDER_BY_OPTIONS, type AdminUsersListQueryParams } from '@/shared';
import {
  adminUsersFilterToParams,
  defaultAdminUsersFilter,
  type AdminUsersFilterFormValues
} from '@/services';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

const SITUATION_OPTIONS = [
  { label: 'Todos', value: 'all' },
  { label: 'Ativos', value: 'active' },
  { label: 'Excluídos', value: 'deleted' }
];

type AdminUsersFilterParams = Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

type AdminUsersFilterFieldsProps = {
  /** Recebe os parâmetros derivados sempre que um filtro muda (busca já com debounce). */
  onChange: (params: AdminUsersFilterParams) => void;
};

export function AdminUsersFilterFields({ onChange }: AdminUsersFilterFieldsProps) {
  const { watch, reset } = useFormContext<AdminUsersFilterFormValues>();

  const search = watch('search');
  const situation = watch('situation');
  const orderBy = watch('orderBy');
  const debouncedSearch = useDebouncedValue(search, 350);

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(adminUsersFilterToParams({ search: debouncedSearch, situation, orderBy }));
  }, [debouncedSearch, situation, orderBy, onChange]);

  return (
    <>
      <InputField name="search" label="Buscar" placeholder="Nome ou e-mail" />
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
