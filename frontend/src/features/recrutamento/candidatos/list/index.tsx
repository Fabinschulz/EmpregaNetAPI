'use client';

import { ApiQueryBoundary, PageHeader, TableContainer, type DataTableColumn } from '@/components';
import { usePersistedTablePagination } from '@/hooks';
import { useCandidatesListQuery, type UserDto } from '@/services';
import { Eye } from 'lucide-react';

const CANDIDATES_COLUMNS: DataTableColumn<UserDto>[] = [
  { key: 'username', header: 'Candidato', render: (candidate) => <strong>{candidate.username}</strong> },
  { key: 'email', header: 'E-mail', render: (candidate) => candidate.email },
  {
    key: 'actions',
    type: 'actions',
    getActions: (candidate) => [
      { key: 'detail', label: 'Detalhes', icon: Eye, href: `/recrutamento/candidatos/${candidate.id}` }
    ]
  }
];

export function RecruitmentCandidatesPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'recrutamento-candidatos' });
  const { data, isPending, isError, error, refetch } = useCandidatesListQuery({
    page: pagination.page,
    size: pagination.pageSize
  });

  return (
    <ApiQueryBoundary
      fallback="candidatos"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidatos"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Recrutamento: Candidatos" description="Listagem de candidatos." />

        <TableContainer
          columns={CANDIDATES_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(candidate) => candidate.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhum candidato"
          emptyMessage="Nenhum candidato encontrado."
        />
      </section>
    </ApiQueryBoundary>
  );
}
