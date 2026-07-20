'use client';

import {
  ApiQueryBoundary,
  Button,
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
  parseApplicationStatus,
  useApplicationsByJobQuery,
  useChangeApplicationStatusMutation,
  useDeleteApplicationMutation,
  type ApplicationStatus,
  type JobApplicationDto
} from '@/features/candidaturas/service';
import { useJobQuery } from '../service';
import { Ban, CheckCircle2, Flag, Pencil, PlayCircle, RotateCcw, Trash2, XCircle, type LucideIcon } from 'lucide-react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useMemo, useState } from 'react';
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

const dateFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' });

function formatCreatedAt(value: string | undefined): string {
  if (!value) return '—';
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? '—' : dateFormatter.format(parsed);
}

type PendingAction =
  | { type: 'status'; application: JobApplicationDto; target: ApplicationStatus }
  | { type: 'delete'; application: JobApplicationDto };

export function CandidatesByJobPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const pagination = usePersistedTablePagination({ storageKey: `recrutamento-vaga-${jobId}-candidatos` });
  const [filters, setFilters] = useState<CandidatesFilterParams>({});
  const [pending, setPending] = useState<PendingAction | null>(null);

  const { data: job } = useJobQuery(jobId);
  const { data, isPending, isError, error, refetch } = useApplicationsByJobQuery(jobId, {
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const { mutate: changeStatus, isPending: isChangingStatus } = useChangeApplicationStatusMutation();
  const { mutate: deleteApplication, isPending: isDeleting } = useDeleteApplicationMutation();

  const handleFilterChange = (next: CandidatesFilterParams) => {
    setFilters(next);
    pagination.setPage(1);
  };

  const columns = useMemo<DataTableColumn<JobApplicationDto>[]>(
    () => [
      { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
      { key: 'candidate', header: 'Candidato', render: (application) => application.candidateId ?? '—' },
      {
        key: 'status',
        header: 'Status',
        render: (application) => <ApplicationStatusBadge status={application.status} />
      },
      { key: 'createdAt', header: 'Recebida em', render: (application) => formatCreatedAt(application.createdAt) },
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
                ? () => setPending({ type: 'status', application, target })
                : () => changeStatus({ id: application.id, status: target })
            };
          });

          actions.push({
            key: 'delete',
            label: 'Excluir',
            icon: Trash2,
            variant: 'destructive',
            disabled: isChangingStatus || isDeleting,
            onSelect: () => setPending({ type: 'delete', application })
          });

          return actions;
        }
      }
    ],
    [changeStatus, isChangingStatus, isDeleting]
  );

  const confirmProps = useMemo(() => {
    if (!pending) return null;
    if (pending.type === 'delete') {
      return {
        title: 'Excluir candidatura',
        description: `A candidatura #${pending.application.id} será removida permanentemente. Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
        cancelLabel: 'Cancelar',
        loading: isDeleting
      };
    }
    const label = applicationTransitionLabels[pending.target];
    return {
      title: `${label} candidatura`,
      description: `Confirmar "${label}" para a candidatura #${pending.application.id}?`,
      confirmLabel: label,
      cancelLabel: 'Voltar',
      loading: isChangingStatus
    };
  }, [pending, isDeleting, isChangingStatus]);

  const handleConfirm = () => {
    if (!pending) return;
    if (pending.type === 'delete') {
      deleteApplication(pending.application.id, { onSuccess: () => setPending(null) });
      return;
    }
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
              <Link href={`/recrutamento/vagas/${jobId}`}>
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

        {confirmProps ? (
          <ConfirmDialog
            open={pending !== null}
            onOpenChange={(open) => {
              if (!open) setPending(null);
            }}
            title={confirmProps.title}
            description={confirmProps.description}
            confirmLabel={confirmProps.confirmLabel}
            cancelLabel={confirmProps.cancelLabel}
            tone="destructive"
            loading={confirmProps.loading}
            onConfirm={handleConfirm}
          />
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
