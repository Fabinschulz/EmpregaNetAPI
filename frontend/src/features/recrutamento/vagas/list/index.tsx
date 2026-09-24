'use client';

import {
  actionIcons,
  ApiQueryBoundary,
  Button,
  ConfirmDialog,
  FilterSection,
  PageHeader,
  StatusBadge,
  TableContainer,
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
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { canManageJob, describeJobStatusBadge, describePositions } from '../domain';
import { jobsRoutes } from '../jobs-routes';
import { useDeleteJobMutation, useJobsListQuery, type JobResponse } from '../service';
import { JobsFilterFields } from './jobs-filter-fields';
import {
  defaultJobsFilter,
  jobsFilterFormSchema,
  jobsFilterToParams,
  type JobsFilterFormValues
} from './jobs-filter-schema';

export function RecruitmentJobsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-vagas' });
  const { setPage } = pagination;
  const {
    values: filter,
    resetKey: filterResetKey,
    onChange: writeFilterToUrl,
    reset: resetFilter
  } = useUrlSyncedParams(defaultJobsFilter, jobsFilterFormSchema);
  const [seenFilterResetKey, setSeenFilterResetKey] = useState(filterResetKey);
  if (filterResetKey !== seenFilterResetKey) {
    setSeenFilterResetKey(filterResetKey);
    setPage(1);
  }

  const { data, isPending, isFetching, isError, error, refetch } = useJobsListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...jobsFilterToParams(filter)
  });

  const handleRefresh = useListRefresh({ refetch, resource: 'vagas' });

  const handleFilterChange = useCallback(
    (next: JobsFilterFormValues) => {
      writeFilterToUrl(next);
      setPage(1);
    },
    [setPage, writeFilterToUrl]
  );

  const handleClearFilter = useCallback(() => resetFilter(defaultJobsFilter), [resetFilter]);

  const hasActiveFilter = hasActiveUrlSyncedParams(filter, defaultJobsFilter);

  const searchOptions = useMemo(
    () => (data?.data ?? []).map((job) => ({ label: job.title, value: String(job.id) })),
    [data]
  );

  const { mutate: deleteJob, isPending: isDeleting } = useDeleteJobMutation();
  const { getDeleteAction, confirmDialogProps } = useRowDeleteAction<JobResponse>({
    permission: 'job.delete',
    resource: 'vaga',
    getId: (job) => job.id,
    getLabel: (job) => job.title,
    deleteById: deleteJob,
    isDeleting,
    getDescription: (job) =>
      `A vaga "${job.title}" e o seu histórico serão removidos permanentemente. Para apenas retirá-la das buscas, use "Encerrar vaga" na edição.`
  });

  const columns = useMemo<DataTableColumn<JobResponse>[]>(
    () => [
      { key: 'title', header: 'Título', render: (job) => <strong>{job.title}</strong> },
      {
        key: 'status',
        header: 'Status',
        render: (job) => {
          const { label, tone } = describeJobStatusBadge(job);
          return <StatusBadge label={label} tone={tone} />;
        }
      },
      {
        key: 'positions',
        header: 'Vagas',
        render: (job) => describePositions(job.positions, job.filledPositions)
      },
      { key: 'createdAt', header: 'Criado em', render: (job) => formatDate(job.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (job) => {
          if (!canManageJob(job)) return [];

          const actions: RowAction[] = [
            { key: 'edit', label: 'Editar', icon: actionIcons.edit, href: jobsRoutes.detail(job.id) }
          ];

          const deleteAction = getDeleteAction(job);
          if (deleteAction) actions.push(deleteAction);

          return actions;
        }
      }
    ],
    [getDeleteAction]
  );

  return (
    <ApiQueryBoundary
      fallback="vagas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vagas"
      onRetry={refetch}
    >
      <section>
        <PageHeader
          title="Vagas"
          description="Gestão de vagas de emprego, incluindo criação, edição e acompanhamento das candidaturas."
          actions={
            <Button variant="primary" asChild>
              <Link href={jobsRoutes.new}>
                <actionIcons.create aria-hidden />
                Nova vaga
              </Link>
            </Button>
          }
        />

        <TableContainer
          columns={columns}
          items={data?.data ?? []}
          getRowKey={(job) => job.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={handleRefresh}
          isRefreshing={isFetching}
          emptyTitle="Nenhuma vaga"
          emptyMessage={
            hasActiveFilter ? (
              <>
                Nenhuma vaga corresponde aos filtros aplicados.{' '}
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
              'Nenhuma vaga cadastrada.'
            )
          }
          filters={
            <FilterSection
              title="Buscar vagas"
              description="Filtre por título/descrição ou pela situação da vaga e escolha a ordem de exibição."
            >
              <FormProvider
                key={filterResetKey}
                validationSchema={jobsFilterFormSchema}
                defaultValues={filter}
                onSubmit={() => undefined}
              >
                <JobsFilterFields
                  onChange={handleFilterChange}
                  searchOptions={searchOptions}
                  searchLoading={isFetching}
                />
              </FormProvider>
            </FilterSection>
          }
        />

        <ConfirmDialog {...confirmDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
