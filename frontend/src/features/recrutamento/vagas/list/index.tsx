'use client';

import {
  ApiQueryBoundary,
  Button,
  ConfirmDialog,
  PageHeader,
  StatusBadge,
  TableContainer,
  TableFilters,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import { formatDate, type JobsListQueryParams } from '@/shared';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import {
  defaultJobsFilter,
  jobsFilterFormSchema,
  useDeleteJobMutation,
  useJobsListQuery,
  type JobDto
} from '../service';
import { JobsFilterFields } from './jobs-filter-fields';

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
  const { getDeleteAction, confirmDialogProps } = useRowDeleteAction<JobDto>({
    permission: 'job.delete',
    resource: 'vaga',
    getId: (job) => job.id,
    getLabel: (job) => job.title,
    deleteById: deleteJob,
    isDeleting,
    getDescription: (job) =>
      `A vaga "${job.title}" e o seu histórico serão removidos permanentemente. Para apenas retirá-la das buscas, use "Encerrar vaga" na edição.`
  });

  const columns = useMemo<DataTableColumn<JobDto>[]>(
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
            { key: 'edit', label: 'Editar', icon: Pencil, href: `/recrutamento/vagas/${job.id}` }
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
              <Link href="/recrutamento/vagas/new">
                <Plus aria-hidden />
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
          onRefresh={() => void refetch()}
          isRefreshing={isFetching}
          emptyTitle="Nenhuma vaga"
          emptyMessage="Nenhuma vaga encontrada para os filtros informados."
          filters={
            <TableFilters title="Buscar vagas" description="Filtre por título/descrição ou pela situação da vaga.">
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
            </TableFilters>
          }
        />

        <ConfirmDialog {...confirmDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
