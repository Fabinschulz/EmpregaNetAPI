'use client';

import { startTransition, useEffect, useState } from 'react';
import { Alert } from '@/components/ui';
import { listAll } from '@/services';
import { useAuth } from '@/features/auth';

export function RecruitmentApplicationsPage() {
  const { token } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<{ id: number; jobId?: number; status?: string | null }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setError(null);
      try {
        const res = await listAll(token, { page: 1, size: 100 });
        if (!mounted) return;
        startTransition(() => {
          setItems(res.data.map((x) => ({ id: x.id, jobId: x.jobId, status: x.status })));
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
      <h1>Recrutamento: Candidaturas</h1>
      <p style={{ color: 'var(--muted)' }}>Listagem de candidaturas (equipe de recrutamento).</p>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}

      {!pending && items.length === 0 ? (
        <Alert title="Nenhuma candidatura">Nenhuma candidatura encontrada.</Alert>
      ) : (
        <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
          {items.map((it) => (
            <div
              key={it.id}
              style={{
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius)',
                padding: 14,
                background: 'rgba(255,255,255,0.05)'
              }}
            >
              <div style={{ fontWeight: 700 }}>#{it.id}</div>
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
