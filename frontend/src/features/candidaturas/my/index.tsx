'use client';

import { ApiQueryBoundary, PageHeader, TableContainer, type DataTableColumn } from '@/components';
import { usePersistedTablePagination } from '@/hooks';
import { useMyJobApplicationsQuery, type JobApplicationDto } from '@/services';
import { ApplicationStatusBadge } from '../application-status-badge';

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
      application.jobId ? [{ key: 'view-job', label: 'Ver vaga', href: `/vagas/${application.jobId}` }] : []
  }
];

export function MyApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'minhas-candidaturas' });
  const { data, isPending, isError, error, refetch } = useMyJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize
  });

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
          emptyTitle="Nenhuma candidatura"
          emptyMessage="Você ainda não se candidatou a nenhuma vaga."
        />
      </section>
    </ApiQueryBoundary>
  );
}
