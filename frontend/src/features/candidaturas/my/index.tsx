'use client';

import {
  actionIcons,
  ApiQueryBoundary,
  Badge,
  ConfirmDialog,
  FilterSection,
  PageHeader,
  TableContainer,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useListRefresh, usePersistedTablePagination } from '@/shared/hooks';
import { type JobApplicationsListQueryParams } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import { Ban } from 'lucide-react';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ApplicationStatusBadge } from '../application-status-badge';
import { canCandidateCancelApplication } from '../domain';
import { useCancelMyApplicationMutation, useMyJobApplicationsQuery, type JobApplicationResponse } from '../service';
import { cancelApplicationDialogCopy } from './cancel-application-dialog-copy';
import {
  hasApplicationStatusChanged,
  markApplicationStatusesAsSeen,
  readLastSeenApplicationStatuses
} from './last-seen-application-status';
import { MyApplicationsFilterFields } from './my-applications-filter-fields';
import {
  defaultMyApplicationsFilter,
  myApplicationsFilterFormSchema,
  myApplicationsFilterToParams
} from './my-applications-filter-schema';
import styles from './my-applications.module.scss';

type MyApplicationsFilterParams = Pick<JobApplicationsListQueryParams, 'status' | 'orderBy'>;

export function MyApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'minhas-candidaturas' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<MyApplicationsFilterParams>(() =>
    myApplicationsFilterToParams(defaultMyApplicationsFilter)
  );
  const [pendingCancelId, setPendingCancelId] = useState<number | null>(null);

  const { data, isPending, isFetching, isError, error, refetch } = useMyJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const lastSeenSnapshotRef = useRef<Record<number, string> | null>(null);
  if (lastSeenSnapshotRef.current === null) {
    lastSeenSnapshotRef.current = readLastSeenApplicationStatuses();
  }

  useEffect(() => {
    const applications = data?.data;
    if (!applications || applications.length === 0) return;
    markApplicationStatusesAsSeen(applications.map(({ id, status }) => ({ id, status })));
  }, [data]);

  const handleRefresh = useListRefresh({ refetch, resource: 'candidaturas' });
  const { mutate: cancelApplication, isPending: isCanceling } = useCancelMyApplicationMutation();

  const handleFiltersChange = useCallback(
    (next: MyApplicationsFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

  const handleConfirmCancel = useCallback(() => {
    if (pendingCancelId === null) return;
    cancelApplication(pendingCancelId, { onSettled: () => setPendingCancelId(null) });
  }, [pendingCancelId, cancelApplication]);

  const columns = useMemo<DataTableColumn<JobApplicationResponse>[]>(
    () => [
      { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
      { key: 'jobId', header: 'Vaga', render: (application) => application.jobTitle },
      {
        key: 'status',
        header: 'Status',
        render: (application) => {
          const isUpdated = hasApplicationStatusChanged(
            lastSeenSnapshotRef.current ?? {},
            application.id,
            application.status
          );

          return (
            <span className={styles.statusCell}>
              <ApplicationStatusBadge status={application.status} audience="candidate" />
              {isUpdated ? (
                <Badge variant="default" aria-label="Status atualizado desde a última visita">
                  Atualizado
                </Badge>
              ) : null}
            </span>
          );
        }
      },
      { key: 'createdAt', header: 'Enviada em', render: (application) => formatDate(application.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (application) => {
          const actions: RowAction[] = [];

          if (application.jobId) {
            actions.push({
              key: 'view-job',
              label: 'Ver vaga',
              icon: actionIcons.details,
              href: `/vagas/${application.jobId}`
            });
          }

          if (canCandidateCancelApplication(application.status)) {
            actions.push({
              key: 'cancel',
              label: 'Cancelar candidatura',
              icon: Ban,
              variant: 'destructive',
              disabled: isCanceling,
              onSelect: () => setPendingCancelId(application.id)
            });
          }

          return actions;
        }
      }
    ],
    [isCanceling]
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
          columns={columns}
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
            <FilterSection title="Filtrar candidaturas" description="Filtre por status e ordenação.">
              <FormProvider
                validationSchema={myApplicationsFilterFormSchema}
                defaultValues={defaultMyApplicationsFilter}
                onSubmit={() => undefined}
              >
                <MyApplicationsFilterFields onChange={handleFiltersChange} />
              </FormProvider>
            </FilterSection>
          }
        />

        <ConfirmDialog
          open={pendingCancelId !== null}
          onOpenChange={(open) => {
            if (!open) setPendingCancelId(null);
          }}
          title={cancelApplicationDialogCopy.title}
          description={pendingCancelId !== null ? cancelApplicationDialogCopy.describe(pendingCancelId) : undefined}
          confirmLabel={cancelApplicationDialogCopy.confirmLabel}
          cancelLabel={cancelApplicationDialogCopy.cancelLabel}
          confirmIcon={Ban}
          tone="destructive"
          loading={isCanceling}
          onConfirm={handleConfirmCancel}
        />
      </section>
    </ApiQueryBoundary>
  );
}
