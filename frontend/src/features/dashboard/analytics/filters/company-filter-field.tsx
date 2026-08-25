'use client';

import { useSelectableCompaniesQuery } from '@/features/recrutamento/vagas/service';
import { SelectField } from '@/shared/components';
import { useMemo } from 'react';
import { ALL_COMPANIES_VALUE } from './dashboard-filter-schema';

export function CompanyFilterField() {
  const companies = useSelectableCompaniesQuery();

  const options = useMemo(
    () => [
      { value: ALL_COMPANIES_VALUE, label: 'Todas as empresas' },
      ...(companies.data ?? []).map((company) => ({ value: String(company.id), label: company.name }))
    ],
    [companies.data]
  );

  return (
    <SelectField
      name="companyId"
      label="Empresa"
      options={options}
      loading={companies.isPending}
      placeholder="Todas as empresas"
    />
  );
}
