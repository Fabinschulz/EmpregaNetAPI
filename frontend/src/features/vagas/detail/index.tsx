'use client';

import {
  Alert,
  ApiQueryBoundary,
  Button,
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
  DetailPageSkeleton,
  StatusBadge
} from '@/components';
import { useAuth } from '@/context';
import { useApplyToJobMutation } from '@/features/candidaturas/service';
import { useJobQuery } from '@/features/recrutamento/vagas/service';
import { Building2, MapPin } from 'lucide-react';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import styles from './job-detail.module.scss';

export function JobDetailPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { isAuthenticated } = useAuth();

  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const { apiError, mutateAsync, isPending: isApplying } = useApplyToJobMutation(jobId);

  function onApply() {
    if (!isAuthenticated) return;
    void mutateAsync();
  }

  const applyLabel = !isAuthenticated
    ? 'Faça login para se candidatar'
    : isApplying
      ? 'Enviando...'
      : 'Candidatar-me';

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
          <Card className={styles.card}>
            <CardHeader>
              <div className={styles.headerRow}>
                <CardTitle>{job.title}</CardTitle>
                <StatusBadge
                  label={job.isActive === false ? 'Encerrada' : 'Ativa'}
                  tone={job.isActive === false ? 'negative' : 'positive'}
                />
              </div>
              <ul className={styles.meta}>
                {job.companyId != null ? (
                  <li className={styles.metaItem}>
                    <Building2 aria-hidden />
                    <span>Empresa #{job.companyId}</span>
                  </li>
                ) : null}
                <li className={styles.metaItem}>
                  <MapPin aria-hidden />
                  <span>{job.location?.trim() ? job.location : 'Local não informado'}</span>
                </li>
              </ul>
            </CardHeader>

            <CardContent>
              <h2 className={styles.sectionLabel}>Descrição</h2>
              <p className={styles.description}>{job.description?.trim() ? job.description : 'Sem descrição.'}</p>
            </CardContent>

            <CardFooter className={styles.footer}>
              <Button variant="primary" onClick={onApply} disabled={!isAuthenticated || isApplying}>
                {applyLabel}
              </Button>
              {!isAuthenticated ? (
                <p className={styles.footerHint}>É necessário entrar na conta para enviar sua candidatura.</p>
              ) : null}
              {apiError ? (
                <Alert variant="destructive" title="Erro">
                  {apiError}
                </Alert>
              ) : null}
            </CardFooter>
          </Card>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
