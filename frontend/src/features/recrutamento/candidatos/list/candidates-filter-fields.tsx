'use client';

import { Button, InputField, SelectField } from '@/components';
import { useFormContext } from '@/context';
import { useDebouncedValue } from '@/hooks';
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
};

export function CandidatesFilterFields({ onChange }: CandidatesFilterFieldsProps) {
  const { watch, reset } = useFormContext<CandidatesFilterFormValues>();

  const search = watch('search');
  const orderBy = watch('orderBy');
  const debouncedSearch = useDebouncedValue(search, 350);

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(candidatesFilterToParams({ search: debouncedSearch, orderBy }));
  }, [debouncedSearch, orderBy, onChange]);

  return (
    <>
      <InputField name="search" label="Buscar" placeholder="Nome ou e-mail" />
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
