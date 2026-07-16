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
import {
  defaultJobsFilter,
  jobsFilterFormSchema,
  jobsFilterToParams,
  useJobsListQuery,
  type JobDto,
  type JobsFilterFormValues
} from '@/services';
import type { JobsListQueryParams } from '@/shared/schema';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';
import { useState } from 'react';
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
  const [filters, setFilters] = useState<JobsFilterParams>({});

  const { data, isPending, isError, error, refetch } = useJobsListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleFilterSubmit = (values: JobsFilterFormValues) => {
    setFilters(jobsFilterToParams(values));
    pagination.setPage(1);
  };

  const handleFilterClear = () => {
    setFilters({});
    pagination.setPage(1);
  };

  return (
    <ApiQueryBoundary
      fallback="vagas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vagas"
      onRetry={() => void refetch()}
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
                onSubmit={handleFilterSubmit}
              >
                <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'flex-end', gap: 12 }}>
                  <JobsFilterFields onClear={handleFilterClear} />
                </div>
              </FormProvider>
            </TableFilters>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
