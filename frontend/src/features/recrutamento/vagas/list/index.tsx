'use client';

import {
    ApiQueryBoundary,
    Button,
    PageHeader,
    StatusBadge,
    TableContainer,
    TableFilters,
    type DataTableColumn
} from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import { defaultJobsFilter, jobsFilterFormSchema, useJobsListQuery, type JobDto } from '@/services';
import type { JobsListQueryParams } from '@/shared/schema';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { JobsFilterFields } from './jobs-filter-fields';

type JobsFilterParams = Pick<JobsListQueryParams, 'search' | 'isActive'>;

const JOBS_COLUMNS: DataTableColumn<JobDto>[] = [
  { key: 'title', header: 'Título', render: (job) => <strong>{job.title}</strong> },
  { key: 'location', header: 'Localização', render: (job) => job.location ?? '—' },
  {
    key: 'status',
    header: 'Status',
    render: (job) => (
      <StatusBadge
        label={job.isActive === false ? 'Encerrada' : 'Ativa'}
        tone={job.isActive === false ? 'negative' : 'positive'}
      />
    )
  },
  {
    key: 'actions',
    type: 'actions',
    getActions: (job) => [{ key: 'edit', label: 'Editar', icon: Pencil, href: `/recrutamento/vagas/${job.id}` }]
  }
];

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
          title="Recrutamento: Vagas"
          description="Gestão de vagas (criar/editar/fechar/excluir)."
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
          columns={JOBS_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(job) => job.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhuma vaga"
          emptyMessage="Nenhuma vaga encontrada para os filtros informados."
          filters={
            <TableFilters title="Buscar vagas" description="Filtre por título/descrição ou pela situação da vaga.">
              <FormProvider
                validationSchema={jobsFilterFormSchema}
                defaultValues={defaultJobsFilter}
                onSubmit={() => undefined}
              >
                <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'flex-end', gap: 12 }}>
                  <JobsFilterFields
                    onChange={handleFiltersChange}
                    searchOptions={searchOptions}
                    searchLoading={isFetching}
                  />
                </div>
              </FormProvider>
            </TableFilters>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
