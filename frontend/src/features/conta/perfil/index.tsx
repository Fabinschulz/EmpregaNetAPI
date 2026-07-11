'use client';

import { ApiQueryBoundary, FormFieldsSkeleton, PageHeader } from '@/components';
import { useAuth } from '@/features/auth';
import { useMeQuery } from '@/services';

export function ProfilePage() {
  const { roles } = useAuth();
  const { data: user, isPending, isError, error, refetch } = useMeQuery();

  return (
    <ApiQueryBoundary
      fallback="perfil"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="perfil"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Minha conta" description="Informações do usuário autenticado." />

        {isPending ? <FormFieldsSkeleton fields={5} /> : null}

        {user ? (
          <article
            style={{
              border: '1px solid var(--border)',
              borderRadius: 'var(--radius)',
              padding: 14,
              background: 'var(--card-bg)'
            }}
          >
            <p>
              <strong>ID:</strong> {user.id}
            </p>
            <p>
              <strong>Usuário:</strong> {user.username}
            </p>
            <p>
              <strong>E-mail:</strong> {user.email}
            </p>
            <p>
              <strong>Roles:</strong> {roles.length ? roles.join(', ') : '—'}
            </p>
          </article>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
