'use client';

import {
  ApiQueryBoundary,
  Button,
  ConfirmDialog,
  PageHeader,
  TableContainer,
  TableFilters,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { ApplicationStatusBadge } from '@/features/candidaturas/application-status-badge';
import {
  applicationStatusTransitions,
  applicationTransitionLabels,
  parseApplicationStatus,
  type ApplicationStatus
} from '@/features/candidaturas/domain';
import {
  useApplicationsByJobQuery,
  useChangeApplicationStatusMutation,
  useDeleteApplicationMutation,
  candidateDisplayName,
  type JobApplicationResponse
} from '@/features/candidaturas/service';
import { usePersistedTablePagination } from '@/shared/hooks';
import { formatDate } from '@/shared/utils';
import { Ban, CheckCircle2, Flag, Pencil, PlayCircle, RotateCcw, XCircle, type LucideIcon } from 'lucide-react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useCallback, useMemo, useState } from 'react';
import { jobsRoutes } from '../jobs-routes';
import { useJobQuery } from '../service';
import {
  CandidatesFilterFields,
  candidatesFilterSchema,
  defaultCandidatesFilter,
  type CandidatesFilterParams
} from './candidates-filter-fields';
import styles from './candidates.module.scss';

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

/** Transições que exigem confirmação por serem terminais/negativas. */
const DESTRUCTIVE_TRANSITIONS: ReadonlySet<ApplicationStatus> = new Set<ApplicationStatus>(['Rejected', 'Canceled']);

type PendingTransition = { application: JobApplicationResponse; target: ApplicationStatus };

export function CandidatesByJobPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const pagination = usePersistedTablePagination({ storageKey: `recrutamento-vaga-${jobId}-candidatos` });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<CandidatesFilterParams>({});
  const [pending, setPending] = useState<PendingTransition | null>(null);

  const { data: job } = useJobQuery(jobId);
  const { data, isPending, isError, error, refetch } = useApplicationsByJobQuery(jobId, {
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const { mutate: changeStatus, isPending: isChangingStatus } = useChangeApplicationStatusMutation();
  const { mutate: deleteApplication, isPending: isDeleting } = useDeleteApplicationMutation();

  const { getDeleteAction, confirmDialogProps: deleteDialogProps } = useRowDeleteAction<JobApplicationResponse>({
    permission: 'jobApplication.delete',
    resource: 'candidatura',
    getId: (application) => application.id,
    getLabel: (application) => `#${application.id}`,
    deleteById: deleteApplication,
    isDeleting,
    getDescription: (application) =>
      `A candidatura #${application.id} será removida permanentemente. Esta ação não pode ser desfeita.`
  });

  const handleFilterChange = useCallback(
    (next: CandidatesFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

  const columns = useMemo<DataTableColumn<JobApplicationResponse>[]>(
    () => [
      { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
      {
        key: 'candidate',
        header: 'Candidato',
        render: (application) => (
          <Link href={`/recrutamento/candidatos/${application.candidate.id}`}>
            {candidateDisplayName(application.candidate)}
          </Link>
        )
      },
      { key: 'candidateEmail', header: 'E-mail', render: (application) => application.candidate.email || '-' },
      {
        key: 'status',
        header: 'Status',
        render: (application) => <ApplicationStatusBadge status={application.status} />
      },
      { key: 'createdAt', header: 'Recebida em', render: (application) => formatDate(application.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (application) => {
          const status = parseApplicationStatus(application.status);
          const transitions = status ? applicationStatusTransitions[status] : [];

          const actions: RowAction[] = transitions.map((target) => {
            const destructive = DESTRUCTIVE_TRANSITIONS.has(target);
            return {
              key: target,
              label: applicationTransitionLabels[target],
              icon: TRANSITION_ICON[target],
              variant: destructive ? 'destructive' : 'default',
              disabled: isChangingStatus || isDeleting,
              onSelect: destructive
                ? () => setPending({ application, target })
                : () => changeStatus({ id: application.id, status: target })
            };
          });

          const deleteAction = getDeleteAction(application);
          if (deleteAction) actions.push(deleteAction);

          return actions;
        }
      }
    ],
    [changeStatus, isChangingStatus, isDeleting, getDeleteAction]
  );

  const pendingLabel = pending ? applicationTransitionLabels[pending.target] : '';

  const handleConfirmTransition = () => {
    if (!pending) return;
    changeStatus({ id: pending.application.id, status: pending.target }, { onSuccess: () => setPending(null) });
  };

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
          title={job?.title ? `Candidatos: ${job.title}` : 'Candidatos da vaga'}
          description="Acompanhe, avance e gerencie as candidaturas desta vaga."
          actions={
            <Button variant="outline" asChild>
              <Link href={jobsRoutes.detail(jobId)}>
                <Pencil aria-hidden />
                Editar vaga
              </Link>
            </Button>
          }
        />

        <TableContainer
          columns={columns}
          items={data?.data ?? []}
          getRowKey={(application) => application.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhuma candidatura"
          emptyMessage="Nenhuma candidatura encontrada para os filtros informados."
          filters={
            <TableFilters
              title="Filtrar candidaturas"
              description="Filtre por status e ordene pela data de recebimento."
            >
              <FormProvider
                validationSchema={candidatesFilterSchema}
                defaultValues={defaultCandidatesFilter}
                onSubmit={() => undefined}
              >
                <div className={styles.filterRow}>
                  <CandidatesFilterFields onChange={handleFilterChange} />
                </div>
              </FormProvider>
            </TableFilters>
          }
        />

        <ConfirmDialog
          open={pending !== null}
          onOpenChange={(open) => {
            if (!open) setPending(null);
          }}
          title={`${pendingLabel} candidatura`}
          description={
            pending ? `Confirmar "${pendingLabel}" para a candidatura #${pending.application.id}?` : undefined
          }
          confirmLabel={pendingLabel}
          cancelLabel="Voltar"
          tone="destructive"
          loading={isChangingStatus}
          onConfirm={handleConfirmTransition}
        />

        <ConfirmDialog {...deleteDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
