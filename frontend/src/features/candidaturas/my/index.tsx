'use client';

import { startTransition, useEffect, useState } from 'react';
import { Alert } from '@/components/ui';
import { listMine } from '@/services';
import { useAuth } from '@/features/auth';

export function MyApplicationsPage() {
  const { token } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<{ id: number; status?: string | null; jobId?: number }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setError(null);
      try {
        const res = await listMine(token, { page: 1, size: 50 });
        if (!mounted) return;
        startTransition(() => {
          setItems(res.data.map((x) => ({ id: x.id, status: x.status, jobId: x.jobId })));
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() => setError(err instanceof Error ? err.message : 'Erro ao carregar candidaturas.'));
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [token]);

  return (
    <div>
      <h1>Minhas candidaturas</h1>
      <p style={{ color: 'var(--muted)' }}>Acompanhe o status das suas candidaturas.</p>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}

      {!pending && items.length === 0 ? (
        <Alert title="Nenhuma candidatura">Você ainda não se candidatou a nenhuma vaga.</Alert>
      ) : (
        <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
          {items.map((it) => (
            <div
              key={it.id}
              style={{
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius)',
                background: 'rgba(255,255,255,0.05)',
                padding: 14
              }}
            >
              <div style={{ fontWeight: 700 }}>Candidatura #{it.id}</div>
              <div style={{ color: 'var(--muted)', fontSize: 14 }}>
                Job: {it.jobId ?? '—'} • Status: {it.status ?? '—'}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
