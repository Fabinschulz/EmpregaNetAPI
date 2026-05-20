'use client';

import { useMemo, useState } from 'react';
import { useParams } from 'next/navigation';
import { Alert, ApiQueryBoundary, Button, DetailPageSkeleton } from '@/components';
import { useAuth } from '@/features/auth';
import { useApplyToJobMutation, useJobQuery } from '@/services';
import { getApiErrorMessage } from '@/utils';

export function JobDetailPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const applyMutation = useApplyToJobMutation();
  const [appMsg, setAppMsg] = useState<string | null>(null);

  function onApply() {
    if (!token) {
      setAppMsg('Você precisa estar logado para se candidatar.');
      return;
    }
    setAppMsg(null);
    applyMutation.mutate(
      { dto: { jobId } },
      {
        onSuccess: (res) => {
          setAppMsg(typeof res === 'string' ? res : 'Candidatura enviada.');
        },
        onError: (err) => {
          setAppMsg(getApiErrorMessage(err, 'candidatura'));
        }
      }
    );
  }

  return (
    <ApiQueryBoundary
      fallback="vaga"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vaga"
      onRetry={() => void refetch()}
    >
      <section>
        {isPending ? <DetailPageSkeleton bodyLines={5} /> : null}
        {job ? (
          <>
            <h1>{job.title}</h1>
            <p style={{ color: 'var(--muted)' }}>{job.description ?? 'Sem descrição.'}</p>

            <p style={{ marginTop: 14, display: 'flex', gap: 10, flexWrap: 'wrap' }}>
              <Button variant="primary" onClick={onApply} disabled={applyMutation.isPending}>
                {applyMutation.isPending ? 'Enviando...' : 'Candidatar-me'}
              </Button>
            </p>
            {appMsg ? (
              <p style={{ marginTop: 12 }}>
                <Alert title="Status">{appMsg}</Alert>
              </p>
            ) : null}
          </>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
