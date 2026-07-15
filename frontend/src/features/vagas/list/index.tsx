'use client';

import {
  Alert,
  ApiQueryBoundary,
  Button,
  ListRowsSkeleton,
  PageHeader,
  TableFilters,
  TablePagination
} from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import { defaultJobsSearch, jobsSearchFormSchema, useJobsListQuery, type JobsSearchFormValues } from '@/services';
import Link from 'next/link';
import { useState } from 'react';
import { JobsSearchFields } from './jobs-search-fields';
import styles from './jobs-list.module.scss';

export function JobsPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'vagas-publicas' });
  const [search, setSearch] = useState<string | undefined>(undefined);

  const { data, isPending, isError, error, refetch } = useJobsListQuery({
    isActive: true,
    search,
    page: pagination.page,
    size: pagination.pageSize
  });
  const items = data?.data ?? [];
  const hasItems = !isPending && items.length > 0;

  const handleSearchSubmit = (values: JobsSearchFormValues) => {
    setSearch(values.search.trim() || undefined);
    pagination.setPage(1);
  };

  const handleSearchClear = () => {
    setSearch(undefined);
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
      <div>
        <PageHeader title="Vagas" description="Encontre a sua próxima oportunidade." />

        <div className={styles.content}>
          <TableFilters title="Buscar vagas" description="Busque por cargo ou palavra-chave.">
            <FormProvider
              validationSchema={jobsSearchFormSchema}
              defaultValues={defaultJobsSearch}
              onSubmit={handleSearchSubmit}
            >
              <JobsSearchFields onClear={handleSearchClear} />
            </FormProvider>
          </TableFilters>

          {isPending ? <ListRowsSkeleton rows={6} /> : null}

          {!isPending && items.length === 0 ? (
            <Alert title="Nenhuma vaga" variant="default">
              {search ? 'Não encontramos vagas para a busca informada.' : 'Não encontramos vagas ativas no momento.'}
            </Alert>
          ) : null}

          {hasItems ? (
            <>
              <ul className={styles.grid} aria-label="Vagas disponíveis">
                {items.map((job) => (
                  <li key={job.id} className={styles.card}>
                    <div className={styles.cardRow}>
                      <div>
                        <div className={styles.cardTitle}>{job.title}</div>
                        <div className={styles.cardLocation}>{job.location ?? '—'}</div>
                      </div>
                      <Button variant="primary" asChild>
                        <Link href={`/vagas/${job.id}`}>Ver detalhes</Link>
                      </Button>
                    </div>
                  </li>
                ))}
              </ul>
              <TablePagination pagination={pagination} totalItems={data?.totalItems} />
            </>
          ) : null}
        </div>
      </div>
    </ApiQueryBoundary>
  );
}
