'use client';

import { APPLICATION_STATUSES, applicationStatusLabels } from '@/features/candidaturas/domain';
import {
  actionIcons,
  AutocompleteField,
  Button,
  FilterBar,
  FilterField,
  SelectField,
  type AutocompleteOption,
  type SelectOption
} from '@/shared/components';
import { useFormContext } from '@/shared/context';
import { useFilterFormSync } from '@/shared/hooks';
import { DATE_ORDER_BY_OPTIONS, LIST_SEARCH_MAX_LENGTH } from '@/shared/schema';
import {
  defaultRecruitmentApplicationsFilter,
  type RecruitmentApplicationsFilterFormValues
} from './recruitment-applications-filter-schema';

const STATUS_OPTIONS: SelectOption[] = [
  { label: 'Todas', value: 'all' },
  ...APPLICATION_STATUSES.map((status) => ({ label: applicationStatusLabels[status], value: status }))
];

const ORDER_BY_OPTIONS: SelectOption[] = [...DATE_ORDER_BY_OPTIONS];

type RecruitmentApplicationsFilterFieldsProps = {
  onChange: (values: RecruitmentApplicationsFilterFormValues) => void;
  searchOptions: AutocompleteOption[];
  searchLoading?: boolean;
};

export function RecruitmentApplicationsFilterFields({
  onChange,
  searchOptions,
  searchLoading
}: RecruitmentApplicationsFilterFieldsProps) {
  const { watch, reset } = useFormContext<RecruitmentApplicationsFilterFormValues>();

  const status = watch('status');
  const search = watch('search');
  const orderBy = watch('orderBy');

  useFilterFormSync({ status, search, orderBy }, onChange);

  return (
    <FilterBar
      actions={
        <Button
          type="button"
          variant="outline"
          startIcon={actionIcons.clearFilters}
          onClick={() => reset(defaultRecruitmentApplicationsFilter)}
        >
          Limpar
        </Button>
      }
    >
      <FilterField span={2}>
        <AutocompleteField
          name="search"
          label="Buscar"
          placeholder="Candidato, e-mail ou vaga"
          options={searchOptions}
          loading={searchLoading}
          maxLength={LIST_SEARCH_MAX_LENGTH}
        />
      </FilterField>
      <SelectField name="status" label="Status" options={STATUS_OPTIONS} />
      <SelectField name="orderBy" label="Ordenar por" options={ORDER_BY_OPTIONS} />
    </FilterBar>
  );
}
