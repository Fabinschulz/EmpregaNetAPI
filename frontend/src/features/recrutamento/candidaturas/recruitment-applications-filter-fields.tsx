'use client';

import { SelectField } from '@/components';
import { useFormContext } from '@/context';
import { type RecruitmentApplicationsFilterFormValues } from '@/services';
import { DATE_ORDER_BY_OPTIONS, type ListOrderByValue } from '@/shared';
import { useEffect, useRef } from 'react';

type RecruitmentApplicationsFilterFieldsProps = {
  onChange: (orderBy: ListOrderByValue) => void;
};

export function RecruitmentApplicationsFilterFields({ onChange }: RecruitmentApplicationsFilterFieldsProps) {
  const { watch } = useFormContext<RecruitmentApplicationsFilterFormValues>();

  const orderBy = watch('orderBy');

  const isFirstRun = useRef(true);
  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }
    onChange(orderBy);
  }, [orderBy, onChange]);

  return <SelectField name="orderBy" label="Ordenar por" options={[...DATE_ORDER_BY_OPTIONS]} />;
}
