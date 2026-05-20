'use client';

import { Alert, ApiQueryBoundary, Button, ListRowsSkeleton } from '@/components';
import { useJobsListQuery } from '@/services';
import Link from 'next/link';

export function RecruitmentJobsPage() {
  const { data, isPending, isError, error, refetch } = useJobsListQuery();
  const jobs = data?.data.map((j) => ({ id: j.id, title: j.title })) ?? [];

  return (
    <ApiQueryBoundary
      fallback="vagas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vagas"
      onRetry={() => void refetch()}
    >
      <section>
        <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', gap: 12 }}>
          <div>
            <h1>Recrutamento: Vagas</h1>
            <p style={{ color: 'var(--muted)' }}>Gestão de vagas (criar/editar/fechar/excluir).</p>
          </div>
          <Button variant="primary" asChild>
            <Link href="/recrutamento/vagas/new">Nova vaga</Link>
          </Button>
        </header>

        {isPending ? <ListRowsSkeleton rows={6} /> : null}

        {!isPending && jobs.length === 0 ? (
          <Alert title="Nenhuma vaga">Ainda não há vagas cadastradas.</Alert>
        ) : (
          <ul style={{ display: 'grid', gap: 10, marginTop: 12, listStyle: 'none', padding: 0 }}>
            {jobs.map((j) => (
              <li
                key={j.id}
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
                <strong>{j.title}</strong>
                <Button asChild>
                  <Link href={`/recrutamento/vagas/${j.id}`}>Editar</Link>
                </Button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
