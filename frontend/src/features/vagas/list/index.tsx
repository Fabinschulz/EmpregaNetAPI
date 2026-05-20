'use client';

import { Alert, ApiQueryBoundary, Button, ListRowsSkeleton } from '@/components';
import { useJobsListQuery } from '@/services';
import Link from 'next/link';

export function JobsPage() {
  const { data, isPending, isError, error, refetch } = useJobsListQuery({ isActive: true });
  const items = data?.data.map((j) => ({ id: j.id, title: j.title, location: j.location })) ?? [];

  return (
    <ApiQueryBoundary
      fallback="vagas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vagas"
      onRetry={() => void refetch()}
    >
      <div>
        <h1>Vagas</h1>
        <p style={{ color: 'var(--muted)' }}>Lista de vagas ativas (leitura pública).</p>

        {isPending ? (
          <ListRowsSkeleton rows={6} />
        ) : items.length === 0 ? (
          <Alert title="Nenhuma vaga" variant="default">
            Não encontramos vagas ativas no momento.
          </Alert>
        ) : (
          <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
            {items.map((j) => (
              <div
                key={j.id}
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  background: 'var(--card-bg)',
                  padding: 14
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
                  <div>
                    <div style={{ fontWeight: 700 }}>{j.title}</div>
                    <div style={{ color: 'var(--muted)', fontSize: 14 }}>{j.location ?? '—'}</div>
                  </div>
                  <Button variant="primary" asChild>
                    <Link href={`/vagas/${j.id}`}>Ver detalhes</Link>
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </ApiQueryBoundary>
  );
}
