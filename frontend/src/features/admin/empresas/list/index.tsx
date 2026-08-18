'use client';

import {
  ApiQueryBoundary,
  Button,
  ConfirmDialog,
  PageHeader,
  TableContainer,
  TableFilters,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { usePersistedTablePagination } from '@/shared/hooks';
import { type CompaniesListQueryParams } from '@/shared/schema';
import { formatDate } from '@/shared/utils';
import { Pencil, Plus } from 'lucide-react';
import Link from 'next/link';
import { useCallback, useMemo, useState } from 'react';
import { useCompaniesListQuery, useDeleteCompanyMutation, type CompanyResponse } from '../service';
import { companiesFilterFormSchema, companiesFilterToParams, defaultCompaniesFilter } from './companies-filter-schema';
import { companiesRoutes } from '../companies-routes';
import { CompaniesFilterFields } from './companies-filter-fields';

type CompaniesFilterParams = Pick<CompaniesListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

export function AdminCompaniesPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'admin-empresas' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<CompaniesFilterParams>(() => companiesFilterToParams(defaultCompaniesFilter));

  const { data, isPending, isFetching, isError, error, refetch } = useCompaniesListQuery({
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

  const searchOptions = useMemo(
    () => (data?.data ?? []).map((company) => ({ label: company.name, value: String(company.id) })),
    [data]
  );

  const { mutate: deleteCompany, isPending: isDeleting } = useDeleteCompanyMutation();
  const { getDeleteAction, confirmDialogProps } = useRowDeleteAction<CompanyResponse>({
    permission: 'company.delete',
    resource: 'empresa',
    getId: (company) => company.id,
    getLabel: (company) => company.name,
    deleteById: deleteCompany,
    isDeleting
  });

  const columns = useMemo<DataTableColumn<CompanyResponse>[]>(
    () => [
      { key: 'name', header: 'Nome', render: (company) => <strong>{company.name}</strong> },
      { key: 'email', header: 'E-mail', render: (company) => company.email ?? '-' },
      { key: 'phone', header: 'Telefone', render: (company) => company.phone ?? '-' },
      { key: 'documentNo', header: 'CNPJ', render: (company) => company.documentNo ?? '-' },
      { key: 'createdAt', header: 'Criado em', render: (company) => formatDate(company.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (company) => {
          const actions: RowAction[] = [
            { key: 'edit', label: 'Editar', icon: Pencil, href: companiesRoutes.detail(company.id) }
          ];

          const deleteAction = getDeleteAction(company);
          if (deleteAction) actions.push(deleteAction);

          return actions;
        }
      }
    ],
    [getDeleteAction]
  );

  return (
    <ApiQueryBoundary
      fallback="empresas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="empresas"
      onRetry={refetch}
    >
      <section>
        <PageHeader
          title="Empresas"
          description="Gestão de empresas (Admin)."
          actions={
            <Button variant="primary" asChild>
              <Link href={companiesRoutes.new}>
                <Plus aria-hidden />
                Nova empresa
              </Link>
            </Button>
          }
        />

        <TableContainer
          columns={columns}
          items={data?.data ?? []}
          getRowKey={(company) => company.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={() => void refetch()}
          isRefreshing={isFetching}
          emptyTitle="Nenhuma empresa"
          emptyMessage="Nenhuma empresa encontrada para os filtros informados."
          filters={
            <TableFilters title="Buscar empresas" description="Filtre por nome/e-mail/CNPJ, situação e ordenação.">
              <FormProvider
                validationSchema={companiesFilterFormSchema}
                defaultValues={defaultCompaniesFilter}
                onSubmit={() => undefined}
              >
                <CompaniesFilterFields
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
