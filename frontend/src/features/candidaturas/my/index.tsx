'use client';

import {
  actionIcons,
  ApiQueryBoundary,
  PageHeader,
  TableContainer,
  TableFilters,
  type DataTableColumn
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useListRefresh, usePersistedTablePagination } from '@/shared/hooks';
import { type JobApplicationsListQueryParams } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import { useCallback, useState } from 'react';
import { ApplicationStatusBadge } from '../application-status-badge';
import { useMyJobApplicationsQuery, type JobApplicationResponse } from '../service';
import { MyApplicationsFilterFields } from './my-applications-filter-fields';
import {
    defaultMyApplicationsFilter,
    myApplicationsFilterFormSchema,
    myApplicationsFilterToParams
} from './my-applications-filter-schema';

type MyApplicationsFilterParams = Pick<JobApplicationsListQueryParams, 'status' | 'orderBy'>;

const MY_APPLICATIONS_COLUMNS: DataTableColumn<JobApplicationResponse>[] = [
  { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
  { key: 'jobId', header: 'Vaga', render: (application) => application.jobId ?? '-' },
  {
    key: 'status',
    header: 'Status',
    render: (application) => <ApplicationStatusBadge status={application.status} />
  },
  { key: 'createdAt', header: 'Enviada em', render: (application) => formatDate(application.createdAt) },
  {
    key: 'actions',
    type: 'actions',
    getActions: (application) =>
      application.jobId ? [{ key: 'view-job', label: 'Ver vaga', icon: actionIcons.details, href: `/vagas/${application.jobId}` }] : []
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

  const handleRefresh = useListRefresh({ refetch, resource: 'candidaturas' });

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
          onRefresh={handleRefresh}
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
