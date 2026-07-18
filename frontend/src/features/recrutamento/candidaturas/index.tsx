'use client';

import {
    ApiQueryBoundary,
    ConfirmDialog,
    PageHeader,
    TableContainer,
    TableFilters,
    type DataTableColumn,
    type RowAction
} from '@/components';
import { FormProvider } from '@/context';
import { ApplicationStatusBadge } from '@/features/candidaturas/application-status-badge';
import { usePersistedTablePagination } from '@/hooks';
import {
    applicationStatusTransitions,
    applicationTransitionLabels,
    defaultRecruitmentApplicationsFilter,
    parseApplicationStatus,
    recruitmentApplicationsFilterFormSchema,
    useAllJobApplicationsQuery,
    useChangeApplicationStatusMutation,
    type ApplicationStatus,
    type JobApplicationDto
} from '@/services';
import type { ListOrderByValue } from '@/shared';
import { Ban, CheckCircle2, Eye, Flag, PlayCircle, RotateCcw, XCircle, type LucideIcon } from 'lucide-react';
import { useCallback, useMemo, useState } from 'react';
import { RecruitmentApplicationsFilterFields } from './recruitment-applications-filter-fields';

/** Ícone da ação que leva a candidatura para cada status alvo. */
const TRANSITION_ICON: Record<ApplicationStatus, LucideIcon> = {
  Pending: RotateCcw,
  Processing: PlayCircle,
  Approved: CheckCircle2,
  Rejected: XCircle,
  Canceled: Ban,
  Finished: Flag,
  Timeout: Ban,
  Error: Ban
};

/** Transições destrutivas exigem confirmação antes de disparar a mutação. */
const DESTRUCTIVE_TRANSITIONS: ReadonlySet<ApplicationStatus> = new Set(['Rejected', 'Canceled']);

type PendingTransition = { id: number; status: ApplicationStatus };

export function RecruitmentApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-candidaturas' });
  const { setPage } = pagination;
  const [orderBy, setOrderBy] = useState<ListOrderByValue | undefined>(defaultRecruitmentApplicationsFilter.orderBy);
  const [pendingTransition, setPendingTransition] = useState<PendingTransition | null>(null);

  const { data, isPending, isFetching, isError, error, refetch } = useAllJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize,
    orderBy
  });
  const { mutate: changeApplicationStatus, isPending: isChangingStatus } = useChangeApplicationStatusMutation();

  const handleOrderByChange = useCallback(
    (next: ListOrderByValue) => {
      setOrderBy(next);
      setPage(1);
    },
    [setPage]
  );

  const handleConfirmTransition = useCallback(() => {
    if (!pendingTransition) return;
    changeApplicationStatus(pendingTransition, { onSettled: () => setPendingTransition(null) });
  }, [pendingTransition, changeApplicationStatus]);

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

          const actions: RowAction[] = transitions.map((target) => {
            const isDestructive = DESTRUCTIVE_TRANSITIONS.has(target);
            return {
              key: target,
              label: applicationTransitionLabels[target],
              icon: TRANSITION_ICON[target],
              onSelect: isDestructive
                ? () => setPendingTransition({ id: application.id, status: target })
                : () => changeApplicationStatus({ id: application.id, status: target }),
              variant: isDestructive ? 'destructive' : 'default',
              disabled: isChangingStatus
            };
          });

          if (application.jobId) {
            actions.push({ key: 'view-job', label: 'Ver vaga', icon: Eye, href: `/vagas/${application.jobId}` });
          }

          return actions;
        }
      }
    ],
    [changeApplicationStatus, isChangingStatus]
  );

  const pendingLabel = pendingTransition ? applicationTransitionLabels[pendingTransition.status] : '';

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
          title="Candidaturas"
          description="Acompanhe e avance as candidaturas pelo processo seletivo."
        />

        <TableContainer
          columns={columns}
          items={data?.data ?? []}
          getRowKey={(application) => application.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={() => void refetch()}
          isRefreshing={isFetching}
          emptyTitle="Nenhuma candidatura"
          emptyMessage="Nenhuma candidatura encontrada."
          filters={
            <TableFilters title="Ordenar candidaturas" description="Escolha a ordem de exibição das candidaturas.">
              <FormProvider
                validationSchema={recruitmentApplicationsFilterFormSchema}
                defaultValues={defaultRecruitmentApplicationsFilter}
                onSubmit={() => undefined}
              >
                <RecruitmentApplicationsFilterFields onChange={handleOrderByChange} />
              </FormProvider>
            </TableFilters>
          }
        />

        <ConfirmDialog
          open={pendingTransition !== null}
          onOpenChange={(open) => {
            if (!open) setPendingTransition(null);
          }}
          title={`${pendingLabel} candidatura?`}
          description={
            pendingTransition
              ? `Esta ação move a candidatura #${pendingTransition.id} para um status final e não pode ser desfeita.`
              : undefined
          }
          confirmLabel={pendingLabel}
          tone="destructive"
          loading={isChangingStatus}
          onConfirm={handleConfirmTransition}
        />
      </section>
    </ApiQueryBoundary>
  );
}
