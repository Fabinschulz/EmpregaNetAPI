import type { JobResponse } from '@/features/recrutamento/vagas/service';
import { Button } from '@/shared/components';
import { CalendarCheck, CheckCircle2, Flag } from 'lucide-react';
import Link from 'next/link';
import styles from './job-closed-notice.module.scss';
import { summarizeJobClosure } from './job-closure';

type JobClosedNoticeProps = {
  job: Pick<JobResponse, 'status' | 'publishedAt' | 'closedAt'>;
};

/**
 * Aviso de vaga encerrada: substitui o CTA de candidatura. Explica que o processo terminou
 * (e não que a página quebrou) e leva o visitante às vagas abertas.
 */
export function JobClosedNotice({ job }: JobClosedNoticeProps) {
  const { headline, reason, publishedOn, closedOn, openPeriod } = summarizeJobClosure(job);
  const Icon = job.status === 'ClosedByFulfillment' ? CheckCircle2 : Flag;

  return (
    <aside className={styles.notice} role="status" aria-labelledby="job-closed-title">
      <span className={styles.icon} aria-hidden>
        <Icon size={28} />
      </span>

      <div className={styles.content}>
        <h2 id="job-closed-title" className={styles.title}>
          Processo seletivo encerrado
        </h2>
        <p className={styles.headline}>{headline}</p>
        <p className={styles.reason}>{reason}</p>

        {publishedOn && closedOn ? (
          <ol className={styles.timeline} aria-label="Período da vaga">
            <li>
              <span className={styles.timelineLabel}>Publicada em</span>
              <span className={styles.timelineValue}>{publishedOn}</span>
            </li>
            <li aria-hidden className={styles.timelineLine} />
            <li>
              <span className={styles.timelineLabel}>
                <CalendarCheck size={14} aria-hidden /> Encerrada em
              </span>
              <span className={styles.timelineValue}>{closedOn}</span>
            </li>
          </ol>
        ) : null}
        {openPeriod ? <p className={styles.period}>{openPeriod}.</p> : null}

        <Button asChild variant="primary" className={styles.cta}>
          <Link href="/vagas">Ver vagas abertas</Link>
        </Button>
      </div>
    </aside>
  );
}
