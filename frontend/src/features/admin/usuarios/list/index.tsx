'use client';

import { ApiQueryBoundary, Badge, PageHeader, TableContainer, type DataTableColumn } from '@/components';
import { usePersistedTablePagination } from '@/hooks';
import { useAdminUsersListQuery, type UserDto } from '@/services';

const USERS_COLUMNS: DataTableColumn<UserDto>[] = [
  { key: 'username', header: 'Usuário', render: (user) => <strong>{user.username}</strong> },
  { key: 'email', header: 'E-mail', render: (user) => user.email },
  { key: 'userType', header: 'Tipo', render: (user) => user.userType ?? '—' },
  {
    key: 'situation',
    header: 'Situação',
    render: (user) => (
      <Badge variant={user.isDeleted ? 'secondary' : 'default'}>{user.isDeleted ? 'Excluído' : 'Ativo'}</Badge>
    )
  },
  {
    key: 'actions',
    type: 'actions',
    getActions: (user) => [{ key: 'detail', label: 'Detalhes', href: `/admin/usuarios/${user.id}` }]
  }
];

export function AdminUsersPage() {
  const pagination = usePersistedTablePagination({ storageKey: 'admin-usuarios' });
  const { data, isPending, isError, error, refetch } = useAdminUsersListQuery({
    page: pagination.page,
    size: pagination.pageSize
  });

  return (
    <ApiQueryBoundary
      fallback="usuários"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="usuários"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Admin: Usuários" description="Gestão de usuários (Admin)." />

        <TableContainer
          columns={USERS_COLUMNS}
          items={data?.data ?? []}
          getRowKey={(user) => user.id}
          pagination={pagination}
          totalItems={data?.totalItems}
          isPending={isPending}
          emptyTitle="Nenhum usuário"
          emptyMessage="Nenhum usuário encontrado."
        />
      </section>
    </ApiQueryBoundary>
  );
}
