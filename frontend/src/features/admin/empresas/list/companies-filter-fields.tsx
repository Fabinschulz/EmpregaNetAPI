'use client';

import { Button, AutocompleteField, SelectField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import {
    companiesFilterToParams,
    defaultCompaniesFilter,
    type CompaniesFilterFormValues
} from '@/services';
import { LIST_ORDER_BY_OPTIONS, type CompaniesListQueryParams } from '@/shared';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

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

  const search = watch('search');
  const situation = watch('situation');
  const orderBy = watch('orderBy');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(companiesFilterToParams({ search, situation, orderBy }));
  }, [search, situation, orderBy, onChange]);

  return (
    <>
      <AutocompleteField
        name="search"
        label="Buscar"
        placeholder="Nome, e-mail ou CNPJ"
        options={searchOptions}
        loading={searchLoading}
      />
      <SelectField name="situation" label="Situação" options={SITUATION_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={[...LIST_ORDER_BY_OPTIONS]} />
      <div style={{ display: 'flex', gap: 8, flex: '0 0 auto' }}>
        <Button type="button" variant="outline" onClick={() => reset(defaultCompaniesFilter)}>
          <X aria-hidden />
          Limpar
        </Button>
      </div>
    </>
  );
}
