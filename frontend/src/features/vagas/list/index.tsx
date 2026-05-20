'use client';

import { ListRowsSkeleton } from '@/components/common';
import { Alert, Button } from '@/components/ui';
import { listJobs } from '@/services';
import Link from 'next/link';
import { startTransition, useEffect, useState } from 'react';

export function JobsPage() {
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<{ id: number; title: string; location?: string | null }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      setPending(true);
      setError(null);
      try {
        const res = await listJobs({ page: 1, size: 100, isActive: true });
        if (!mounted) return;
        startTransition(() => {
          setData(res.data.map((j) => ({ id: j.id, title: j.title, location: j.location })));
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() => setError(err instanceof Error ? err.message : 'Erro ao carregar vagas.'));
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, []);

  return (
    <div>
      <h1>Vagas</h1>
      <p style={{ color: 'var(--muted)' }}>Lista de vagas ativas (leitura pública).</p>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}

      {pending ? (
        <ListRowsSkeleton rows={6} />
      ) : data.length === 0 ? (
        <Alert title="Nenhuma vaga" variant="default">
          Não encontramos vagas ativas no momento.
        </Alert>
      ) : (
        <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
          {data.map((j) => (
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
  );
}
