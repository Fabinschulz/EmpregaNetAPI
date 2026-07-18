'use client';

import { ApiQueryBoundary, PageHeader, TableContainer, TableFilters, type DataTableColumn } from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import {
  defaultMyApplicationsFilter,
  myApplicationsFilterFormSchema,
  myApplicationsFilterToParams,
  useMyJobApplicationsQuery,
  type JobApplicationDto
} from '@/services';
import type { JobApplicationsListQueryParams } from '@/shared';
import { Eye } from 'lucide-react';
import { useCallback, useState } from 'react';
import { ApplicationStatusBadge } from '../application-status-badge';
import { MyApplicationsFilterFields } from './my-applications-filter-fields';

type MyApplicationsFilterParams = Pick<JobApplicationsListQueryParams, 'status' | 'orderBy'>;

const MY_APPLICATIONS_COLUMNS: DataTableColumn<JobApplicationDto>[] = [
  { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
  { key: 'jobId', header: 'Vaga', render: (application) => application.jobId ?? '—' },
  {
    key: 'status',
    header: 'Status',
    render: (application) => <ApplicationStatusBadge status={application.status} />
  },
  {
    key: 'actions',
    type: 'actions',
    getActions: (application) =>
      application.jobId ? [{ key: 'view-job', label: 'Ver vaga', icon: Eye, href: `/vagas/${application.jobId}` }] : []
  }
];

export function MyApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'minhas-candidaturas' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<MyApplicationsFilterParams>(() =>
    myApplicationsFilterToParams(defaultMyApplicationsFilter)
  );

  const { data, isPending, isFetching, isError, error, refetch } = useMyJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleFiltersChange = useCallback(
    (next: MyApplicationsFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

  return (
    <ApiQueryBoundary
      fallback="candidaturas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidaturas"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Minhas candidaturas" description="Acompanhe o status das suas candidaturas." />

        <TableContainer
          columns={MY_APPLICATIONS_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(application) => application.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={() => void refetch()}
          isRefreshing={isFetching}
          emptyTitle="Nenhuma candidatura"
          emptyMessage="Nenhuma candidatura encontrada para os filtros informados."
          filters={
            <TableFilters title="Filtrar candidaturas" description="Filtre por status e ordenação.">
              <FormProvider
                validationSchema={myApplicationsFilterFormSchema}
                defaultValues={defaultMyApplicationsFilter}
                onSubmit={() => undefined}
              >
                <MyApplicationsFilterFields onChange={handleFiltersChange} />
              </FormProvider>
            </TableFilters>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
