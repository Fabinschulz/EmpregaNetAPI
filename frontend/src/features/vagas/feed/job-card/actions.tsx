'use client';

import { useApplyToJobMutation } from '@/features/candidaturas/service';
import { Button, Spinner } from '@/shared/components';
import { useAuth } from '@/shared/context';
import { useCopyToClipboard } from '@/shared/hooks';
import { ArrowRight, Check, Link2, LogIn } from 'lucide-react';
import Link from 'next/link';
import { publicJobsRoutes, publicJobUrl } from '../../public-jobs-routes';
import styles from './job-card.module.scss';

type JobCardActionsProps = {
  jobId: number;
  jobTitle: string;
  hasApplied?: boolean;
  /** Vaga encerrada não aceita candidatura; sobra a leitura do detalhe. */
  isActive?: boolean;
};

export function JobCardActions({ jobId, jobTitle, hasApplied, isActive = true }: JobCardActionsProps) {
  const { isAuthenticated } = useAuth();
  const { mutate, isPending } = useApplyToJobMutation(jobId);
  const copyLink = useCopyToClipboard({
    successTitle: 'Link copiado',
    successDescription: 'Cole onde quiser para compartilhar esta vaga.'
  });

  return (
    <div className={styles.actions}>
      <Button
        type="button"
        variant="ghost"
        size="icon"
        className={styles.iconAction}
        onClick={() => void copyLink(publicJobUrl(jobId))}
        title="Copiar link da vaga"
        aria-label={`Copiar link da vaga ${jobTitle}`}
      >
        <Link2 aria-hidden />
      </Button>

      <Button variant="outline" asChild>
        <Link href={publicJobsRoutes.detail(jobId)} aria-label={`Ver detalhes da vaga ${jobTitle}`}>
          Ver detalhes
          <ArrowRight aria-hidden />
        </Link>
      </Button>

      {hasApplied ? (
        <span className={styles.appliedBadge}>
          <Check className={styles.appliedIcon} aria-hidden />
          Candidatura enviada
        </span>
      ) : !isActive ? null : isAuthenticated ? (
        <Button
          variant="primary"
          onClick={() => mutate()}
          disabled={isPending}
          aria-busy={isPending}
          aria-label={`Candidatar-se à vaga ${jobTitle}`}
        >
          {isPending ? <Spinner size="sm" /> : null}
          Candidatar-se
        </Button>
      ) : (
        <Button variant="primary" asChild>
          <Link href="/login" aria-label={`Entrar para se candidatar à vaga ${jobTitle}`}>
            <LogIn aria-hidden />
            Entrar para se candidatar
          </Link>
        </Button>
      )}
    </div>
  );
}
