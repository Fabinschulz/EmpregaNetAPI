'use client';

import { APPLICATION_STATUSES, applicationStatusLabels } from '@/features/candidaturas/domain';
import {
    actionIcons,
    Button,
    FilterBar,
    FilterField,
    InputField,
    MultiSelectField,
    SelectField
} from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { useFilterFormSync } from '@/shared/hooks';
import { jobAreaVocabulary, UF_OPTIONS } from '@/shared/schema';
import { DASHBOARD_PERIODS, dashboardPeriodLabels, type DashboardFilters } from '../../service';
import { CompanyFilterField } from './company-filter-field';
import {
    ALL_STATUSES_VALUE,
    dashboardFilterFormToFilters,
    defaultDashboardFilterForm,
    type DashboardFilterFormValues
} from './dashboard-filter-schema';

const PERIOD_OPTIONS = DASHBOARD_PERIODS.map((period) => ({
  value: period,
  label: dashboardPeriodLabels[period]
}));

const STATUS_OPTIONS = [
  { value: ALL_STATUSES_VALUE, label: 'Todos os status' },
  ...APPLICATION_STATUSES.map((status) => ({ value: status, label: applicationStatusLabels[status] }))
];

const STATE_OPTIONS = UF_OPTIONS.map((uf) => ({ value: uf.value, label: uf.label }));
const AREA_OPTIONS = jobAreaVocabulary.options.map((area) => ({ value: area.value, label: area.label }));

export type DashboardFiltersProps = {
  onChange: (filters: DashboardFilters) => void;
  canSelectCompany: boolean;
};

export function DashboardFiltersBar({ onChange, canSelectCompany }: DashboardFiltersProps) {
  const { watch, reset } = useFormContext<DashboardFilterFormValues>();
  const values = watch();

  const filters = dashboardFilterFormToFilters(values);

  useFilterFormSync(filters, (next) => {
    if (next) onChange(next);
  });

  const isCustomPeriod = values.period === 'Custom';

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultDashboardFilterForm)}
        >
          Limpar
        </Button>
      }
    >
      <SelectField name="period" label="Período" options={PERIOD_OPTIONS} />

      {isCustomPeriod ? (
        <>
          <InputField name="from" label="De" type="date" />
          <InputField name="to" label="Até" type="date" />
        </>
      ) : null}

      {canSelectCompany ? <CompanyFilterField /> : null}

      <MultiSelectField name="states" label="Localização (UF)" options={STATE_OPTIONS} placeholder="Todas as UFs" />

      <MultiSelectField name="areas" label="Área" options={AREA_OPTIONS} placeholder="Todas as áreas" />

      <FilterField span={2}>
        <SelectField
          name="applicationStatus"
          label="Status da candidatura"
          options={STATUS_OPTIONS}
          hint="Recorta apenas números de candidatura; não afeta vagas, usuários nem empresas."
        />
      </FilterField>
    </FilterBar>
  );
}
