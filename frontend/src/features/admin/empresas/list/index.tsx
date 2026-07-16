'use client';

import { ApiQueryBoundary, Button, PageHeader, TableContainer, type DataTableColumn } from '@/components';
import { usePersistedTablePagination } from '@/hooks';
import { useCompaniesListQuery, type CompanyDto } from '@/services';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';

const COMPANIES_COLUMNS: DataTableColumn<CompanyDto>[] = [
  { key: 'name', header: 'Nome', render: (company) => <strong>{company.name}</strong> },
  { key: 'email', header: 'E-mail', render: (company) => company.email ?? '—' },
  { key: 'phone', header: 'Telefone', render: (company) => company.phone ?? '—' },
  { key: 'documentNo', header: 'CNPJ', render: (company) => company.documentNo ?? '—' },
  {
    key: 'actions',
    type: 'actions',
    getActions: (company) => [{ key: 'edit', label: 'Editar', icon: Pencil, href: `/admin/empresas/${company.id}` }]
  }
];

export function AdminCompaniesPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'admin-empresas' });
  const { data, isPending, isError, error, refetch } = useCompaniesListQuery({
    page: pagination.page,
    size: pagination.pageSize
  });

  return (
    <ApiQueryBoundary
      fallback="empresas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="empresas"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader
          title="Admin: Empresas"
          description="Gestão de empresas (Admin)."
          actions={
            <Button variant="primary" asChild>
              <Link href="/admin/empresas/new">
                <Plus aria-hidden />
                Nova empresa
              </Link>
            </Button>
          }
        />

        <TableContainer
          columns={COMPANIES_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(company) => company.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhuma empresa"
          emptyMessage="Nenhuma empresa encontrada."
        />
      </section>
    </ApiQueryBoundary>
  );
}
