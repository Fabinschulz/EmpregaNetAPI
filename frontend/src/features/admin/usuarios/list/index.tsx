'use client';

import {
    ApiQueryBoundary,
    PageHeader,
    StatusBadge,
    TableContainer,
    TableFilters,
    type DataTableColumn
} from '@/components';
import { FormProvider } from '@/context';
import { usePersistedTablePagination } from '@/hooks';
import {
    adminUsersFilterFormSchema,
    adminUsersFilterToParams,
    defaultAdminUsersFilter,
    useAdminUsersListQuery,
    type UserDto
} from '@/services';
import { userTypeLabel, type AdminUsersListQueryParams } from '@/shared';
import { Eye } from 'lucide-react';
import { useCallback, useMemo, useState } from 'react';
import { AdminUsersFilterFields } from './admin-users-filter-fields';

type AdminUsersFilterParams = Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

const USERS_COLUMNS: DataTableColumn<UserDto>[] = [
  { key: 'username', header: 'Usuário', render: (user) => <strong>{user.username}</strong> },
  { key: 'email', header: 'E-mail', render: (user) => user.email },
  { key: 'userType', header: 'Tipo', render: (user) => userTypeLabel(user.userType) },
  {
    key: 'situation',
    header: 'Situação',
    render: (user) => (
      <StatusBadge label={user.isDeleted ? 'Excluído' : 'Ativo'} tone={user.isDeleted ? 'negative' : 'positive'} />
    )
  },
  {
    key: 'actions',
    type: 'actions',
    getActions: (user) => [{ key: 'detail', label: 'Detalhes', icon: Eye, href: `/admin/usuarios/${user.id}` }]
  }
];

export function AdminUsersPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'admin-usuarios' });
  const { setPage } = pagination;
  const [filters, setFilters] = useState<AdminUsersFilterParams>(() =>
    adminUsersFilterToParams(defaultAdminUsersFilter)
  );

  const { data, isPending, isFetching, isError, error, refetch } = useAdminUsersListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...filters
  });

  const handleFiltersChange = useCallback(
    (next: AdminUsersFilterParams) => {
      setFilters(next);
      setPage(1);
    },
    [setPage]
  );

  const searchOptions = useMemo(
    () => (data?.data ?? []).map((user) => ({ label: user.username, value: String(user.id) })),
    [data]
  );

  return (
    <ApiQueryBoundary
      fallback="usuários"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="usuários"
      onRetry={refetch}
    >
      <section>
        <PageHeader title="Usuários" description="Gestão de usuários (Admin)." />

        <TableContainer
          columns={USERS_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(user) => user.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          onRefresh={() => void refetch()}
          isRefreshing={isFetching}
          emptyTitle="Nenhum usuário"
          emptyMessage="Nenhum usuário encontrado para os filtros informados."
          filters={
            <TableFilters title="Buscar usuários" description="Filtre por nome/e-mail, situação e ordenação.">
              <FormProvider
                validationSchema={adminUsersFilterFormSchema}
                defaultValues={defaultAdminUsersFilter}
                onSubmit={() => undefined}
              >
                <AdminUsersFilterFields
                  onChange={handleFiltersChange}
                  searchOptions={searchOptions}
                  searchLoading={isFetching}
                />
              </FormProvider>
            </TableFilters>
          }
        />
      </section>
    </ApiQueryBoundary>
  );
}
