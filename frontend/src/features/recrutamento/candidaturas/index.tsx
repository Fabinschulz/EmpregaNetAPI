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
  Button,
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
import {
  hasActiveUrlSyncedParams,
  useListRefresh,
  usePersistedTablePagination,
  useUrlSyncedParams
} from '@/shared/hooks';
import { formatDate } from '@/shared/utils';
import { MousePointerClick } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { RecruitmentApplicationsFilterFields } from './recruitment-applications-filter-fields';
import {
  defaultRecruitmentApplicationsFilter,
  recruitmentApplicationsFilterFormSchema,
  recruitmentApplicationsFilterToParams,
  type RecruitmentApplicationsFilterFormValues
} from './recruitment-applications-filter-schema';
import styles from './recruitment-applications.module.scss';

const DESTRUCTIVE_TRANSITIONS: ReadonlySet<ApplicationStatus> = new Set(['Rejected', 'Canceled']);
type PendingTransition = { application: JobApplicationResponse; target: ApplicationStatus };

export function RecruitmentApplicationsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-candidaturas' });
  const { setPage } = pagination;
  const {
    values: filter,
    resetKey: filterResetKey,
    onChange: writeFilterToUrl,
    reset: resetFilter
  } = useUrlSyncedParams(defaultRecruitmentApplicationsFilter, recruitmentApplicationsFilterFormSchema);
  const [seenFilterResetKey, setSeenFilterResetKey] = useState(filterResetKey);
  if (filterResetKey !== seenFilterResetKey) {
    setSeenFilterResetKey(filterResetKey);
    setPage(1);
  }
  const [pendingTransition, setPendingTransition] = useState<PendingTransition | null>(null);

  const { data, isPending, isFetching, isError, error, refetch } = useAllJobApplicationsQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...recruitmentApplicationsFilterToParams(filter)
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

  const handleFilterChange = useCallback(
    (next: RecruitmentApplicationsFilterFormValues) => {
      writeFilterToUrl(next);
      setPage(1);
    },
    [setPage, writeFilterToUrl]
  );

  const handleClearFilter = useCallback(() => resetFilter(defaultRecruitmentApplicationsFilter), [resetFilter]);

  const hasActiveFilter = hasActiveUrlSyncedParams(filter, defaultRecruitmentApplicationsFilter);

  const searchOptions = useMemo(() => {
    const labels = new Set<string>();
    for (const application of data?.data ?? []) {
      labels.add(candidateDisplayName(application.candidate));
      if (application.candidate.email) labels.add(application.candidate.email);
      if (application.jobTitle) labels.add(application.jobTitle);
    }
    return [...labels].map((label) => ({ label, value: label }));
  }, [data]);

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
          emptyMessage={
            hasActiveFilter ? (
              <>
                Nenhuma candidatura corresponde aos filtros aplicados.{' '}
                <Button
                  type="button"
                  variant="link"
                  size="sm"
                  startIcon={actionIcons.clearFilters}
                  onClick={handleClearFilter}
                >
                  Limpar filtros
                </Button>
              </>
            ) : (
              'Nenhuma candidatura encontrada.'
            )
          }
          filters={
            <FilterSection
              title="Filtrar candidaturas"
              description="Filtre por status, busque por candidato, e-mail ou vaga e escolha a ordem de exibição."
            >
              <FormProvider
                key={filterResetKey}
                validationSchema={recruitmentApplicationsFilterFormSchema}
                defaultValues={filter}
                onSubmit={() => undefined}
              >
                <RecruitmentApplicationsFilterFields
                  onChange={handleFilterChange}
                  searchOptions={searchOptions}
                  searchLoading={isFetching}
                />
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
