'use client';

import { Alert, ApiQueryBoundary, ListRowsSkeleton } from '@/components';
import { useMyJobApplicationsQuery } from '@/services';

export function MyApplicationsPage() {
  const { data, isPending, isError, error, refetch } = useMyJobApplicationsQuery();
  const items = data?.data.map((x) => ({ id: x.id, status: x.status, jobId: x.jobId })) ?? [];

  return (
    <ApiQueryBoundary
      fallback="candidaturas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidaturas"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Minhas candidaturas</h1>
        <p style={{ color: 'var(--muted)' }}>Acompanhe o status das suas candidaturas.</p>

        {isPending ? <ListRowsSkeleton rows={5} /> : null}

        {!isPending && items.length === 0 ? (
          <Alert title="Nenhuma candidatura">Você ainda não se candidatou a nenhuma vaga.</Alert>
        ) : (
          <ul style={{ display: 'grid', gap: 10, marginTop: 12, listStyle: 'none', padding: 0 }}>
            {items.map((it) => (
              <li
                key={it.id}
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  background: 'var(--card-bg)',
                  padding: 14
                }}
              >
                <strong>Candidatura #{it.id}</strong>
                <p style={{ color: 'var(--muted)', fontSize: 14, margin: '4px 0 0' }}>
                  Job: {it.jobId ?? '—'} • Status: {it.status ?? '—'}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
