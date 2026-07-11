'use client';

import { ApiQueryBoundary, PageHeader, TableContainer, type DataTableColumn, type RowAction } from '@/components';
import { ApplicationStatusBadge } from '@/features/candidaturas/application-status-badge';
import { usePersistedTablePagination } from '@/hooks';
import {
  applicationStatusTransitions,
  applicationTransitionLabels,
  parseApplicationStatus,
  useAllJobApplicationsQuery,
  useChangeApplicationStatusMutation,
  type JobApplicationDto
} from '@/services';
import { useMemo } from 'react';

export function RecruitmentApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-candidaturas' });
  const { data, isPending, isError, error, refetch } = useAllJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize
  });
  const { mutate: changeApplicationStatus, isPending: isChangingStatus } = useChangeApplicationStatusMutation();

  const columns = useMemo<DataTableColumn<JobApplicationDto>[]>(
    () => [
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
        getActions: (application) => {
          const status = parseApplicationStatus(application.status);
          const transitions = status ? applicationStatusTransitions[status] : [];

          const actions: RowAction[] = transitions.map((target) => ({
            key: target,
            label: applicationTransitionLabels[target],
            onSelect: () => changeApplicationStatus({ id: application.id, status: target }),
            variant: target === 'Rejected' || target === 'Canceled' ? 'destructive' : 'default',
            disabled: isChangingStatus
          }));

          if (application.jobId) {
            actions.push({ key: 'view-job', label: 'Ver vaga', href: `/vagas/${application.jobId}` });
          }

          return actions;
        }
      }
    ],
    [changeApplicationStatus, isChangingStatus]
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
        <PageHeader
          title="Recrutamento: Candidaturas"
          description="Acompanhe e avance as candidaturas pelo processo seletivo."
        />

        <TableContainer
          columns={columns}
          items={data?.data ?? []}
          getRowKey={(application) => application.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhuma candidatura"
          emptyMessage="Nenhuma candidatura encontrada."
        />
      </section>
    </ApiQueryBoundary>
  );
}
