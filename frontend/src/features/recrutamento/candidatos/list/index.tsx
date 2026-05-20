'use client';

import { Alert, ApiQueryBoundary, Button, ListRowsSkeleton } from '@/components';
import { useCandidatesListQuery } from '@/services';
import Link from 'next/link';

export function RecruitmentCandidatesPage() {
  const { data, isPending, isError, error, refetch } = useCandidatesListQuery();
  const items = data?.data.map((c) => ({ id: c.id, username: c.username, email: c.email })) ?? [];

  return (
    <ApiQueryBoundary
      fallback="candidatos"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidatos"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Recrutamento: Candidatos</h1>
        <p style={{ color: 'var(--muted)' }}>Listagem de candidatos.</p>

        {isPending ? <ListRowsSkeleton rows={6} /> : null}

        {!isPending && items.length === 0 ? (
          <Alert title="Nenhum candidato">Nenhum candidato encontrado.</Alert>
        ) : (
          <ul style={{ display: 'grid', gap: 10, marginTop: 12, listStyle: 'none', padding: 0 }}>
            {items.map((c) => (
              <li
                key={c.id}
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
                  <strong>{c.username}</strong>
                  <br />
                  <span style={{ color: 'var(--muted)', fontSize: 14 }}>{c.email}</span>
                </span>
                <Button asChild>
                  <Link href={`/recrutamento/candidatos/${c.id}`}>Detalhes</Link>
                </Button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
