'use client';

import { Button, FilterBar, SelectField } from '@/components';
import { useFormContext } from '@/context';
import {
  APPLICATION_STATUSES,
  applicationStatusLabels,
  defaultMyApplicationsFilter,
  myApplicationsFilterToParams,
  type MyApplicationsFilterFormValues
} from '../service';
import { DATE_ORDER_BY_OPTIONS, type JobApplicationsListQueryParams } from '@/shared';
import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';

const STATUS_OPTIONS = [
  { label: 'Todas', value: 'all' },
  ...APPLICATION_STATUSES.map((status) => ({ label: applicationStatusLabels[status], value: status }))
];

type MyApplicationsFilterParams = Pick<JobApplicationsListQueryParams, 'status' | 'orderBy'>;

type MyApplicationsFilterFieldsProps = {
  onChange: (params: MyApplicationsFilterParams) => void;
};

export function MyApplicationsFilterFields({ onChange }: MyApplicationsFilterFieldsProps) {
  const { watch, reset } = useFormContext<MyApplicationsFilterFormValues>();

  const status = watch('status');
  const orderBy = watch('orderBy');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(myApplicationsFilterToParams({ status, orderBy }));
  }, [status, orderBy, onChange]);

  return (
    <FilterBar
      actions={
        <Button type="button" variant="outline" onClick={() => reset(defaultMyApplicationsFilter)}>
          <X aria-hidden />
          Limpar
        </Button>
      }
    >
      <SelectField name="status" label="Status" options={STATUS_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={[...DATE_ORDER_BY_OPTIONS]} />
    </FilterBar>
  );
}
