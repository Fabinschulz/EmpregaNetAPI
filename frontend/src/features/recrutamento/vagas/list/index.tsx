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
import { useListRefresh, usePersistedTablePagination } from '@/shared/hooks';
import { type JobsListQueryParams } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { jobsRoutes } from '../jobs-routes';
import { useDeleteJobMutation, useJobsListQuery, type JobResponse } from '../service';
import { JobsFilterFields } from './jobs-filter-fields';
import { defaultJobsFilter, jobsFilterFormSchema } from './jobs-filter-schema';

type JobsFilterParams = Pick<JobsListQueryParams, 'search' | 'isActive'>;

export function RecruitmentJobsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-vagas' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<JobsFilterParams>({});

  const { data, isPending, isFetching, isError, error, refetch } = useJobsListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleRefresh = useListRefresh({ refetch, resource: 'vagas' });

  const handleFiltersChange = useCallback(
    (next: JobsFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

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
        render: (job) => (
          <StatusBadge label={job.isActive ? 'Ativa' : 'Encerrada'} tone={job.isActive ? 'positive' : 'negative'} />
        )
      },
      { key: 'createdAt', header: 'Criado em', render: (job) => formatDate(job.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (job) => {
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
          emptyMessage="Nenhuma vaga encontrada para os filtros informados."
          filters={
            <FilterSection title="Buscar vagas" description="Filtre por título/descrição ou pela situação da vaga.">
              <FormProvider
                validationSchema={jobsFilterFormSchema}
                defaultValues={defaultJobsFilter}
                onSubmit={() => undefined}
              >
                <JobsFilterFields
                  onChange={handleFiltersChange}
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
