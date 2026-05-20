'use client';

import { ApiQueryBoundary, FormFieldsSkeleton } from '@/components';
import { useCandidateQuery } from '@/services';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';

export function CandidateDetailPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: user, isPending, isError, error, refetch } = useCandidateQuery(id);

  return (
    <ApiQueryBoundary
      fallback="candidato"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidato"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Candidato</h1>
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
          </article>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
