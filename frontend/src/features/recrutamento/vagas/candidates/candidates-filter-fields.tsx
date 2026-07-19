'use client';

import { SelectField, type SelectOption } from '@/components';
import { useFormContext } from '@/context';
import { APPLICATION_STATUSES, applicationStatusLabels } from '@/features/candidaturas/service';
import { DATE_ORDER_BY_OPTIONS, LIST_ORDER_BY_VALUES } from '@/shared';
import { useEffect, useRef } from 'react';
import { z } from 'zod';

/** Valores do filtro de status: os status do processo seletivo + "Todas". */
export const candidatesStatusFilterValues = ['all', ...APPLICATION_STATUSES] as const;

export const candidatesFilterSchema = z.object({
  status: z.enum(candidatesStatusFilterValues),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type CandidatesFilterFormValues = z.infer<typeof candidatesFilterSchema>;

export const defaultCandidatesFilter: CandidatesFilterFormValues = {
  status: 'all',
  orderBy: 'createdAt_DESC'
};

/** Parâmetros derivados aplicados à query de candidaturas da vaga. */
export type CandidatesFilterParams = { status?: string; orderBy?: string };

export function candidatesFilterToParams(values: CandidatesFilterFormValues): CandidatesFilterParams {
  return {
    status: values.status === 'all' ? undefined : values.status,
    orderBy: values.orderBy
  };
}

const STATUS_OPTIONS: SelectOption[] = [
  { label: 'Todas', value: 'all' },
  ...APPLICATION_STATUSES.map((status) => ({ label: applicationStatusLabels[status], value: status }))
];

const ORDER_BY_OPTIONS: SelectOption[] = [...DATE_ORDER_BY_OPTIONS];

type CandidatesFilterFieldsProps = {
  /** Recebe os parâmetros derivados sempre que um filtro muda. */
  onChange: (params: CandidatesFilterParams) => void;
};

export function CandidatesFilterFields({ onChange }: CandidatesFilterFieldsProps) {
  const { watch } = useFormContext<CandidatesFilterFormValues>();

  const status = watch('status');
  const orderBy = watch('orderBy');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(candidatesFilterToParams({ status, orderBy }));
  }, [status, orderBy, onChange]);

  return (
    <>
      <SelectField name="status" label="Status" options={STATUS_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={ORDER_BY_OPTIONS} />
    </>
  );
}
