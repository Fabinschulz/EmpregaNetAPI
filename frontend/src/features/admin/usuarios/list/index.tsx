'use client';

import { Alert, ApiQueryBoundary, Button, ListRowsSkeleton } from '@/components';
import { useAdminUsersListQuery } from '@/services';
import Link from 'next/link';

export function AdminUsersPage() {
  const { data, isPending, isError, error, refetch } = useAdminUsersListQuery();
  const items = data?.data.map((u) => ({ id: u.id, username: u.username, email: u.email })) ?? [];

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
        <h1>Admin: Usuários</h1>
        <p style={{ color: 'var(--muted)' }}>Gestão de usuários (Admin).</p>

        {isPending ? <ListRowsSkeleton rows={6} /> : null}

        {!isPending && items.length === 0 ? (
          <Alert title="Nenhum usuário">Nenhum usuário encontrado.</Alert>
        ) : (
          <ul style={{ display: 'grid', gap: 10, marginTop: 12, listStyle: 'none', padding: 0 }}>
            {items.map((u) => (
              <li
                key={u.id}
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  padding: 14,
                  background: 'var(--card-bg)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: 12
                }}
              >
                <span>
                  <strong>{u.username}</strong>
                  <br />
                  <span style={{ color: 'var(--muted)', fontSize: 14 }}>{u.email}</span>
                </span>
                <Button asChild>
                  <Link href={`/admin/usuarios/${u.id}`}>Detalhes</Link>
                </Button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
