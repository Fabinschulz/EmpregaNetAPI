'use client';

import { Alert, ApiQueryBoundary, Button, DetailPageSkeleton } from '@/components';
import { useAuth } from '@/features/auth';
import { useApplyToJobMutation, useJobQuery } from '@/services';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';

export function JobDetailPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const { apiError, mutateAsync, isPending: isApplying } = useApplyToJobMutation(jobId);

  function onApply() {
    if (!token) return;
    void mutateAsync();
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
              <Button variant="primary" onClick={onApply} disabled={!token || isApplying}>
                {!token ? 'Faça login para se candidatar' : isApplying ? 'Enviando...' : 'Candidatar-me'}
              </Button>
            </p>
            {apiError ? (
              <p style={{ marginTop: 12 }}>
                <Alert variant="destructive" title="Erro">
                  {apiError}
                </Alert>
              </p>
            ) : null}
          </>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
