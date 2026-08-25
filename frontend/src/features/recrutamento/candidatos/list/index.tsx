'use client';

import {
    actionIcons,
    ApiQueryBoundary,
    FilterSection,
    PageHeader,
    TableContainer,
    type DataTableColumn
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useListRefresh, usePersistedTablePagination } from '@/shared/hooks';
import { type CandidatesListQueryParams, type UserResponse } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import { useCallback, useMemo, useState } from 'react';
import { useCandidatesListQuery } from '../service';
import { CandidatesFilterFields } from './candidates-filter-fields';
import {
    candidatesFilterFormSchema,
    candidatesFilterToParams,
    defaultCandidatesFilter
} from './candidates-filter-schema';

type CandidatesFilterParams = Pick<CandidatesListQueryParams, 'search' | 'orderBy'>;

const CANDIDATES_COLUMNS: DataTableColumn<UserResponse>[] = [
  { key: 'username', header: 'Candidato', render: (candidate) => <strong>{candidate.username}</strong> },
  { key: 'email', header: 'E-mail', render: (candidate) => candidate.email },
  { key: 'createdAt', header: 'Cadastrado em', render: (candidate) => formatDate(candidate.createdAt) },
  {
    key: 'actions',
    type: 'actions',
    getActions: (candidate) => [
      { key: 'detail', label: 'Detalhes', icon: actionIcons.details, href: `/recrutamento/candidatos/${candidate.id}` }
    ]
  }
];

export function RecruitmentCandidatesPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-candidatos' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<CandidatesFilterParams>(() =>
    candidatesFilterToParams(defaultCandidatesFilter)
  );

  const { data, isPending, isFetching, isError, error, refetch } = useCandidatesListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleRefresh = useListRefresh({ refetch, resource: 'candidatos' });

  const handleFiltersChange = useCallback(
    (next: CandidatesFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

  const searchOptions = useMemo(
    () => (data?.data ?? []).map((candidate) => ({ label: candidate.username, value: String(candidate.id) })),
    [data]
  );

  return (
    <ApiQueryBoundary
      fallback="candidatos"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidatos"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Candidatos" description="Listagem de candidatos." />

        <TableContainer
          columns={CANDIDATES_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(candidate) => candidate.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={handleRefresh}
          isRefreshing={isFetching}
          emptyTitle="Nenhum candidato"
          emptyMessage="Nenhum candidato encontrado para os filtros informados."
          filters={
            <FilterSection title="Buscar candidatos" description="Filtre por nome/e-mail e ordenação.">
              <FormProvider
                validationSchema={candidatesFilterFormSchema}
                defaultValues={defaultCandidatesFilter}
                onSubmit={() => undefined}
              >
                <CandidatesFilterFields
                  onChange={handleFiltersChange}
                  searchOptions={searchOptions}
                  searchLoading={isFetching}
                />
              </FormProvider>
            </FilterSection>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
