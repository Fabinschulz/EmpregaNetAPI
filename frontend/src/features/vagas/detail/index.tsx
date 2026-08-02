'use client';

import { Alert, Button, Card, CardContent, CardFooter, CardHeader, CardTitle, StatusBadge } from '@/components';
import { useAuth } from '@/context';
import { useApplyToJobMutation } from '@/features/candidaturas/service';
import type { JobDto } from '@/features/recrutamento/vagas/service';
import { Building2 } from 'lucide-react';
import styles from './job-detail.module.scss';

type JobDetailPageProps = {
  /** Vaga já resolvida no servidor (Server Component). Este componente não busca dados. */
  job: JobDto;
};

/**
 * Apresentação da vaga + ação de candidatura. Os dados chegam via prop (fluxo único de
 * obtenção no servidor, `getJobCached`).
 */
export function JobDetailPage({ job }: JobDetailPageProps) {
  const { isAuthenticated } = useAuth();
  const { apiError, mutateAsync, isPending: isApplying } = useApplyToJobMutation(job.id);

  function onApply() {
    if (!isAuthenticated) return;
    void mutateAsync();
  }

  const applyLabel = !isAuthenticated ? 'Faça login para se candidatar' : isApplying ? 'Enviando...' : 'Candidatar-me';

  return (
    <section>
      <Card className={styles.card}>
        <CardHeader>
          <div className={styles.headerRow}>
            <CardTitle>{job.title}</CardTitle>
            <StatusBadge label={job.isActive ? 'Ativa' : 'Encerrada'} tone={job.isActive ? 'positive' : 'negative'} />
          </div>
          <ul className={styles.meta}>
            {job.companyId != null ? (
              <li className={styles.metaItem}>
                <Building2 aria-hidden />
                <span>Empresa #{job.companyId}</span>
              </li>
            ) : null}
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
    </section>
  );
}
