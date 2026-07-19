'use client';

import { ApiQueryBoundary, FormFieldsSkeleton, PageHeader } from '@/components';
import { useCandidateQuery } from '../service';
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
        <PageHeader title="Candidato" description="Ficha do candidato." />
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
