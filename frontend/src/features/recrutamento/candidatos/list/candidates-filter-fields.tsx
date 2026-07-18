'use client';

import { Button, AutocompleteField, SelectField, type AutocompleteOption } from '@/components';
import { useFormContext } from '@/context';
import {
    candidatesFilterToParams,
    defaultCandidatesFilter,
    type CandidatesFilterFormValues
} from '@/services';
import { LIST_ORDER_BY_OPTIONS, type CandidatesListQueryParams } from '@/shared';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

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

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(candidatesFilterToParams({ search, orderBy }));
  }, [search, orderBy, onChange]);

  return (
    <>
      <AutocompleteField
        name="search"
        label="Buscar"
        placeholder="Nome ou e-mail"
        options={searchOptions}
        loading={searchLoading}
      />
      <SelectField name="orderBy" label="Ordenar por" options={[...LIST_ORDER_BY_OPTIONS]} />
      <div style={{ display: 'flex', gap: 8, flex: '0 0 auto' }}>
        <Button type="button" variant="outline" onClick={() => reset(defaultCandidatesFilter)}>
          <X aria-hidden />
          Limpar
        </Button>
      </div>
    </>
  );
}
