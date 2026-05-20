'use client';

import { Alert, ApiQueryBoundary, ListRowsSkeleton } from '@/components';
import { useAllJobApplicationsQuery } from '@/services';

export function RecruitmentApplicationsPage() {
  const { data, isPending, isError, error, refetch } = useAllJobApplicationsQuery();
  const items = data?.data.map((x) => ({ id: x.id, jobId: x.jobId, status: x.status })) ?? [];

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
        <h1>Recrutamento: Candidaturas</h1>
        <p style={{ color: 'var(--muted)' }}>Listagem de candidaturas (equipe de recrutamento).</p>

        {isPending ? <ListRowsSkeleton rows={6} /> : null}

        {!isPending && items.length === 0 ? (
          <Alert title="Nenhuma candidatura">Nenhuma candidatura encontrada.</Alert>
        ) : (
          <ul style={{ display: 'grid', gap: 10, marginTop: 12, listStyle: 'none', padding: 0 }}>
            {items.map((it) => (
              <li
                key={it.id}
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  padding: 14,
                  background: 'var(--card-bg)'
                }}
              >
                <strong>#{it.id}</strong>
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
