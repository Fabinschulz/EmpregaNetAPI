'use client';

import {
  ApiQueryBoundary,
  ConfirmDialog,
  PageHeader,
  StatusBadge,
  TableContainer,
  TableFilters,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { usePersistedTablePagination } from '@/shared/hooks';
import { type UserResponse, type AdminUsersListQueryParams } from '@/shared/schema';
import { formatDate, userTypeLabel } from '@/shared/utils';
import { Eye } from 'lucide-react';
import { useCallback, useMemo, useState } from 'react';
import { useAdminUsersListQuery, useDeleteAdminUserMutation } from '../service';
import {
  adminUsersFilterFormSchema,
  adminUsersFilterToParams,
  defaultAdminUsersFilter
} from './admin-users-filter-schema';
import { adminUsersRoutes } from '../admin-users-routes';
import { AdminUsersFilterFields } from './admin-users-filter-fields';

type AdminUsersFilterParams = Pick<AdminUsersListQueryParams, 'search' | 'isDeleted' | 'orderBy'>;

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

  const { mutate: deleteUser, isPending: isDeleting } = useDeleteAdminUserMutation();
  const { getDeleteAction, confirmDialogProps } = useRowDeleteAction<UserResponse>({
    permission: 'user.delete',
    resource: 'usuário',
    getId: (user) => user.id,
    getLabel: (user) => user.username,
    deleteById: deleteUser,
    isDeleting,
    getDescription: (user) =>
      `O usuário "${user.username}" será marcado como excluído e perderá o acesso. O registro permanece para auditoria.`
  });

  const columns = useMemo<DataTableColumn<UserResponse>[]>(
    () => [
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
      { key: 'createdAt', header: 'Criado em', render: (user) => formatDate(user.createdAt) },
      {
        key: 'actions',
        type: 'actions',
        getActions: (user) => {
          const actions: RowAction[] = [
            { key: 'detail', label: 'Detalhes', icon: Eye, href: adminUsersRoutes.detail(user.id) }
          ];

          const deleteAction = user.isDeleted ? null : getDeleteAction(user);
          if (deleteAction) actions.push(deleteAction);

          return actions;
        }
      }
    ],
    [getDeleteAction]
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
        <PageHeader title="Usuários" description="Gestão de usuários." />

        <TableContainer
          columns={columns}
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

        <ConfirmDialog {...confirmDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
