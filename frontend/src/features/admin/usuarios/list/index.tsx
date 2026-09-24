'use client';

import {
  actionIcons,
  ApiQueryBoundary,
  Button,
  ConfirmDialog,
  FilterSection,
  PageHeader,
  StatusBadge,
  TableContainer,
  useRowDeleteAction,
  type DataTableColumn,
  type RowAction
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import {
  hasActiveUrlSyncedParams,
  useListRefresh,
  usePersistedTablePagination,
  useUrlSyncedParams
} from '@/shared/hooks';
import { type UserResponse } from '@/shared/schema';
import { formatDate, maskCpf, userTypeLabel } from '@/shared/utils';
import { useCallback, useMemo, useState } from 'react';
import { adminUsersRoutes } from '../admin-users-routes';
import { useAdminUsersListQuery, useDeleteAdminUserMutation } from '../service';
import { AdminUsersFilterFields } from './admin-users-filter-fields';
import {
  adminUsersFilterFormSchema,
  adminUsersFilterToParams,
  defaultAdminUsersFilter,
  type AdminUsersFilterFormValues
} from './admin-users-filter-schema';

export function AdminUsersPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'admin-usuarios' });
  const { setPage } = pagination;
  const {
    values: filter,
    resetKey: filterResetKey,
    onChange: writeFilterToUrl,
    reset: resetFilter
  } = useUrlSyncedParams(defaultAdminUsersFilter, adminUsersFilterFormSchema);

  const [seenFilterResetKey, setSeenFilterResetKey] = useState(filterResetKey);
  if (filterResetKey !== seenFilterResetKey) {
    setSeenFilterResetKey(filterResetKey);
    setPage(1);
  }

  const { data, isPending, isFetching, isError, error, refetch } = useAdminUsersListQuery({
    page: pagination.page,
    size: pagination.pageSize,
    ...adminUsersFilterToParams(filter)
  });

  const handleRefresh = useListRefresh({ refetch, resource: 'usuários' });

  const handleFilterChange = useCallback(
    (next: AdminUsersFilterFormValues) => {
      writeFilterToUrl(next);
      setPage(1);
    },
    [setPage, writeFilterToUrl]
  );

  const handleClearFilter = useCallback(() => resetFilter(defaultAdminUsersFilter), [resetFilter]);

  const hasActiveFilter = hasActiveUrlSyncedParams(filter, defaultAdminUsersFilter);

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
      { key: 'cpf', header: 'CPF', render: (user) => (user.cpf ? maskCpf(user.cpf) : '-') },
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
            { key: 'detail', label: 'Detalhes', icon: actionIcons.details, href: adminUsersRoutes.detail(user.id) }
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
          onRefresh={handleRefresh}
          isRefreshing={isFetching}
          emptyTitle="Nenhum usuário"
          emptyMessage={
            hasActiveFilter ? (
              <>
                Nenhum usuário corresponde aos filtros aplicados.{' '}
                <Button
                  type="button"
                  variant="link"
                  size="sm"
                  startIcon={actionIcons.clearFilters}
                  onClick={handleClearFilter}
                >
                  Limpar filtros
                </Button>
              </>
            ) : (
              'Nenhum usuário cadastrado.'
            )
          }
          filters={
            <FilterSection
              title="Buscar usuários"
              description="Filtre por nome/e-mail/CPF, situação, tipo de usuário e ordenação."
            >
              <FormProvider
                key={filterResetKey}
                validationSchema={adminUsersFilterFormSchema}
                defaultValues={filter}
                onSubmit={() => undefined}
              >
                <AdminUsersFilterFields
                  onChange={handleFilterChange}
                  searchOptions={searchOptions}
                  searchLoading={isFetching}
                />
              </FormProvider>
            </FilterSection>
          }
        />

        <ConfirmDialog {...confirmDialogProps} />
      </section>
    </ApiQueryBoundary>
  );
}
