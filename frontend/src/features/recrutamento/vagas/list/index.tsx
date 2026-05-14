'use client';

import { startTransition, useEffect, useState } from 'react';
import Link from 'next/link';
import { ListRowsSkeleton } from '@/components/common';
import { Alert, Button } from '@/components/ui';
import { listJobs } from '@/services';
import { useAuth } from '@/features/auth';

export function RecruitmentJobsPage() {
  const { token } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [jobs, setJobs] = useState<{ id: number; title: string }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      setPending(true);
      setError(null);
      try {
        // GET /api/jobs é AllowAnonymous, mas aqui é gestão (listagem completa).
        const res = await listJobs({ page: 1, size: 100 });
        if (!mounted) return;
        startTransition(() => {
          setJobs(res.data.map((j) => ({ id: j.id, title: j.title })));
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
  }, [token]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', gap: 12 }}>
        <div>
          <h1>Recrutamento: Vagas</h1>
          <p style={{ color: 'var(--muted)' }}>Gestão de vagas (criar/editar/fechar/excluir).</p>
        </div>
        <Button variant="primary" asChild>
          <Link href="/recrutamento/vagas/new">Nova vaga</Link>
        </Button>
      </div>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <ListRowsSkeleton rows={6} /> : null}

      {!pending && jobs.length === 0 ? (
        <Alert title="Nenhuma vaga">Ainda não há vagas cadastradas.</Alert>
      ) : (
        <div style={{ display: 'grid', gap: 10, marginTop: 12 }}>
          {jobs.map((j) => (
            <div
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
              <div style={{ fontWeight: 700 }}>{j.title}</div>
              <Button asChild>
                <Link href={`/recrutamento/vagas/${j.id}`}>Editar</Link>
              </Button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
