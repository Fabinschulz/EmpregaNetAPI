'use client';

import { startTransition, useEffect, useState } from 'react';
import Link from 'next/link';
import { Alert, Button } from '@/components/ui';
import { listCompanies } from '@/services';
import { useAuth } from '@/features/auth';

export function AdminCompaniesPage() {
  const { token } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<{ id: number; name: string }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setError(null);
      try {
        const res = await listCompanies(token, { page: 1, size: 100 });
        if (!mounted) return;
        startTransition(() => {
          setItems(res.data.map((c) => ({ id: c.id, name: c.name })));
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() => setError(err instanceof Error ? err.message : 'Erro ao carregar empresas.'));
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
      <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', gap: 12 }}>
        <div>
          <h1>Admin: Empresas</h1>
          <p style={{ color: 'var(--muted)' }}>Gestão de empresas (Admin).</p>
        </div>
        <Button variant="primary" asChild>
          <Link href="/admin/empresas/new">Nova empresa</Link>
        </Button>
      </div>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}

      {!pending && items.length === 0 ? (
        <Alert title="Nenhuma empresa">Nenhuma empresa encontrada.</Alert>
      ) : (
        <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
          {items.map((c) => (
            <div
              key={c.id}
              style={{
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius)',
                padding: 14,
                background: 'rgba(255,255,255,0.05)',
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
          ))}
        </div>
      )}
    </div>
  );
}
