'use client';

import {
    actionIcons,
    ApiQueryBoundary,
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
import { useCallback, useMemo, useState } from 'react';
import { ApplicationStatusBadge } from '../application-status-badge';
import { canCandidateCancelApplication } from '../domain';
import { useCancelMyApplicationMutation, useMyJobApplicationsQuery, type JobApplicationResponse } from '../service';
import { cancelApplicationDialogCopy } from './cancel-application-dialog-copy';
import { MyApplicationsFilterFields } from './my-applications-filter-fields';
import {
    defaultMyApplicationsFilter,
    myApplicationsFilterFormSchema,
    myApplicationsFilterToParams
} from './my-applications-filter-schema';

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
      { key: 'jobId', header: 'Vaga', render: (application) => application.jobId ?? '-' },
      {
        key: 'status',
        header: 'Status',
        render: (application) => <ApplicationStatusBadge status={application.status} audience="candidate" />
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
