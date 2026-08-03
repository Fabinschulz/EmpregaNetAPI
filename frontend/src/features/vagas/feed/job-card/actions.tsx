'use client';

import { Button, Spinner } from '@/components';
import { useAuth } from '@/context';
import { useApplyToJobMutation } from '@/features/candidaturas/service';
import { ArrowRight, Check, LogIn } from 'lucide-react';
import Link from 'next/link';
import styles from './job-card.module.scss';

type JobCardActionsProps = {
  jobId: number;
  jobTitle: string;
  hasApplied?: boolean;
};

export function JobCardActions({ jobId, jobTitle, hasApplied }: JobCardActionsProps) {
  const { isAuthenticated } = useAuth();
  const { mutate, isPending } = useApplyToJobMutation(jobId);

  return (
    <div className={styles.actions}>
      <Button variant="outline" size="sm" asChild>
        <Link href={`/vagas/${jobId}`} aria-label={`Ver detalhes da vaga ${jobTitle}`}>
          Ver detalhes
          <ArrowRight aria-hidden />
        </Link>
      </Button>

      {hasApplied ? (
        <span className={styles.appliedBadge}>
          <Check className={styles.chipIcon} aria-hidden />
          Candidatura enviada
        </span>
      ) : isAuthenticated ? (
        <Button
          variant="primary"
          size="sm"
          onClick={() => mutate()}
          disabled={isPending}
          aria-busy={isPending}
          aria-label={`Candidatar-se à vaga ${jobTitle}`}
        >
          {isPending ? <Spinner size="sm" /> : null}
          Candidatar-se
        </Button>
      ) : (
        <Button variant="primary" size="sm" asChild>
          <Link href="/login" aria-label={`Entrar para se candidatar à vaga ${jobTitle}`}>
            <LogIn aria-hidden />
            Entrar para se candidatar
          </Link>
        </Button>
      )}
    </div>
  );
}
