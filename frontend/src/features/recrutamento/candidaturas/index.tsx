'use client';

import { ApplicationStatusBadge } from '@/features/candidaturas/application-status-badge';
import {
  applicationTransitionDialogTitle,
  describeApplicationTransitionConfirmation
} from '@/features/candidaturas/application-transition-copy';
import {
  applicationStatusTransitions,
  applicationTransitionIcons,
  applicationTransitionLabels,
  canDeleteApplication,
  parseApplicationStatus,
  type ApplicationStatus
} from '@/features/candidaturas/domain';
import {
  candidateDisplayName,
  useAllJobApplicationsQuery,
  useChangeApplicationStatusMutation,
  useDeleteApplicationMutation,
  type JobApplicationResponse
} from '@/features/candidaturas/service';
import {
  actionIcons,
  ApiQueryBoundary,
  ConfirmDialog,
  FilterSection,
  PageHeader,
  TableContainer,
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useListRefresh, usePersistedTablePagination } from '@/shared/hooks';
import { type ListOrderByValue } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import { MousePointerClick } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { RecruitmentApplicationsFilterFields } from './recruitment-applications-filter-fields';
import {
  defaultRecruitmentApplicationsFilter,
  recruitmentApplicationsFilterFormSchema
} from './recruitment-applications-filter-schema';
import styles from './recruitment-applications.module.scss';

const DESTRUCTIVE_TRANSITIONS: ReadonlySet<ApplicationStatus> = new Set(['Rejected', 'Canceled']);
type PendingTransition = { application: JobApplicationResponse; target: ApplicationStatus };

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

  const handleRefresh = useListRefresh({ refetch, resource: 'candidaturas' });
  const { mutate: changeApplicationStatus, isPending: isChangingStatus } = useChangeApplicationStatusMutation();
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

  const handleOrderByChange = useCallback(
    (next: ListOrderByValue) => {
      setOrderBy(next);
      setPage(1);
    },
    [setPage]
  );

  const handleConfirmTransition = useCallback(() => {
    if (!pendingTransition) return;
    changeApplicationStatus(
      { id: pendingTransition.application.id, status: pendingTransition.target },
      { onSettled: () => setPendingTransition(null) }
    );
  }, [pendingTransition, changeApplicationStatus]);

  const columns = useMemo<DataTableColumn<JobApplicationResponse>[]>(
    () => [
      { key: 'id', header: 'Candidatura', render: (application) => <strong>#{application.id}</strong> },
      {
        key: 'candidate',
        header: 'Candidato',
        render: (application) => (
          <Tooltip>
            <TooltipTrigger asChild>
              <Link href={`/recrutamento/candidatos/${application.candidate.id}`} className={styles.candidateLink}>
                {candidateDisplayName(application.candidate)}
                <MousePointerClick className={styles.candidateLinkIcon} aria-hidden />
              </Link>
            </TooltipTrigger>
            <TooltipContent>Ver perfil do candidato</TooltipContent>
          </Tooltip>
        )
      },
      { key: 'candidateEmail', header: 'E-mail', render: (application) => application.candidate.email || '-' },
      { key: 'jobId', header: 'Vaga', render: (application) => application.jobTitle },
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
            const isDestructive = DESTRUCTIVE_TRANSITIONS.has(target);
            return {
              key: target,
              label: applicationTransitionLabels[target],
              icon: applicationTransitionIcons[target],
              onSelect: () => setPendingTransition({ application, target }),
              variant: isDestructive ? 'destructive' : 'default',
              disabled: isChangingStatus || isDeleting
            };
          });

          if (application.jobId) {
            actions.push({
              key: 'view-job',
              label: 'Ver vaga',
              icon: actionIcons.view,
              href: `/vagas/${application.jobId}`
            });
          }

          const deleteAction = canDeleteApplication(application.status) ? getDeleteAction(application) : null;
          if (deleteAction) actions.push(deleteAction);

          return actions;
        }
      }
    ],
    [isChangingStatus, isDeleting, getDeleteAction]
  );

  const pendingLabel = pendingTransition ? applicationTransitionLabels[pendingTransition.target] : '';
  const pendingCandidateName = pendingTransition ? candidateDisplayName(pendingTransition.application.candidate) : '';
  const pendingCurrentStatus = pendingTransition ? parseApplicationStatus(pendingTransition.application.status) : null;

  return (
    <ApiQueryBoundary
      fallback="candidaturas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidaturas"
      onRetry={refetch}
    >
      <section>
        <PageHeader title="Candidaturas" description="Acompanhe e avance as candidaturas pelo processo seletivo." />

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
          emptyMessage="Nenhuma candidatura encontrada."
          filters={
            <FilterSection title="Ordenar candidaturas" description="Escolha a ordem de exibição das candidaturas.">
              <FormProvider
                validationSchema={recruitmentApplicationsFilterFormSchema}
                defaultValues={defaultRecruitmentApplicationsFilter}
                onSubmit={() => undefined}
              >
                <RecruitmentApplicationsFilterFields onChange={handleOrderByChange} />
              </FormProvider>
            </FilterSection>
          }
        />

        <ConfirmDialog
          open={pendingTransition !== null}
          onOpenChange={(open) => {
            if (!open) setPendingTransition(null);
          }}
          title={
            pendingTransition ? applicationTransitionDialogTitle(pendingTransition.target, pendingCandidateName) : ''
          }
          description={
            pendingTransition
              ? describeApplicationTransitionConfirmation(pendingCurrentStatus, pendingTransition.target)
              : undefined
          }
          confirmLabel={pendingLabel}
          tone={pendingTransition && DESTRUCTIVE_TRANSITIONS.has(pendingTransition.target) ? 'destructive' : 'default'}
          loading={isChangingStatus}
          onConfirm={handleConfirmTransition}
        />

        <ConfirmDialog {...deleteDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
