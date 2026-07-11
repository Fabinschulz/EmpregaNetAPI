'use client';

import { Alert, ApiQueryBoundary, Button, ListRowsSkeleton, PageHeader } from '@/components';
import { useCompaniesListQuery } from '@/services';
import Link from 'next/link';

export function AdminCompaniesPage() {
  const { data, isPending, isError, error, refetch } = useCompaniesListQuery();
  const items = data?.data.map((c) => ({ id: c.id, name: c.name })) ?? [];

  return (
    <ApiQueryBoundary
      fallback="empresas"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="empresas"
      onRetry={() => void refetch()}
    >
      <div>
        <PageHeader
          title="Admin: Empresas"
          description="Gestão de empresas (Admin)."
          actions={
            <Button variant="primary" asChild>
              <Link href="/admin/empresas/new">Nova empresa</Link>
            </Button>
          }
        />

        {isPending ? <ListRowsSkeleton rows={6} /> : null}

        {!isPending && items.length === 0 ? (
          <Alert title="Nenhuma empresa">Nenhuma empresa encontrada.</Alert>
        ) : (
          <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
            {items.map((c) => (
              <div key={c.id}>
                <div
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
                  <div style={{ fontWeight: 700 }}>{c.name}</div>
                  <Button asChild>
                    <Link href={`/admin/empresas/${c.id}`}>Editar</Link>
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
