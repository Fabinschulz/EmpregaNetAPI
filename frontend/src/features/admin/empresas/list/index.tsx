'use client';

import {
  ApiQueryBoundary,
  Button,
  PageHeader,
  TableContainer,
  TableFilters,
  type DataTableColumn
} from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import {
  companiesFilterFormSchema,
  companiesFilterToParams,
  defaultCompaniesFilter,
  useCompaniesListQuery,
  type CompanyDto
} from '@/services';
import type { CompaniesListQueryParams } from '@/shared';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useState } from 'react';
import { CompaniesFilterFields } from './companies-filter-fields';

type CompaniesFilterParams = Pick<CompaniesListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

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
  const { setPage } = pagination;
  const [filters, setFilters] = useState<CompaniesFilterParams>(() =>
    companiesFilterToParams(defaultCompaniesFilter)
  );

  const { data, isPending, isError, error, refetch } = useCompaniesListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleFiltersChange = useCallback(
    (next: CompaniesFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

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
          emptyMessage="Nenhuma empresa encontrada para os filtros informados."
          filters={
            <TableFilters title="Buscar empresas" description="Filtre por nome/e-mail/CNPJ, situação e ordenação.">
              <FormProvider
                validationSchema={companiesFilterFormSchema}
                defaultValues={defaultCompaniesFilter}
                onSubmit={() => undefined}
              >
                <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'flex-end', gap: 12 }}>
                  <CompaniesFilterFields onChange={handleFiltersChange} />
                </div>
              </FormProvider>
            </TableFilters>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
