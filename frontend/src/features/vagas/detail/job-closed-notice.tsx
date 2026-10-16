import type { JobResponse } from '@/features/recrutamento/vagas/service';
import { Button } from '@/shared/components';
import { ArrowRight, CalendarCheck, CalendarPlus, CheckCircle2, Flag, Hourglass } from 'lucide-react';
import Link from 'next/link';
import styles from './job-closed-notice.module.scss';
import { summarizeJobClosure } from './job-closure';

type JobClosedNoticeProps = {
  job: Pick<JobResponse, 'status' | 'positions' | 'publishedAt' | 'closedAt'>;
};

export function JobClosedNotice({ job }: JobClosedNoticeProps) {
  const { statusLabel, title, headline, reason, publishedOn, closedOn, openPeriod } = summarizeJobClosure(job);
  const fulfilled = job.status === 'ClosedByFulfillment';
  const Icon = fulfilled ? CheckCircle2 : Flag;

  return (
    <aside
      className={styles.notice}
      data-tone={fulfilled ? 'success' : 'neutral'}
      role="status"
      aria-labelledby="job-closed-title"
    >
      <span className={styles.icon} aria-hidden>
        <Icon size={32} />
      </span>

      <div className={styles.content}>
        <span className={styles.pill}>{statusLabel}</span>
        <h2 id="job-closed-title" className={styles.title}>
          {title}
        </h2>
        <p className={styles.headline}>{headline}</p>
        <p className={styles.reason}>{reason}</p>

        {publishedOn && closedOn ? (
          <ol className={styles.stepper} aria-label="Período da vaga">
            <li className={styles.step}>
              <span className={styles.dot} aria-hidden>
                <CalendarPlus size={14} />
              </span>
              <span className={styles.stepLabel}>Publicada em</span>
              <span className={styles.stepValue}>{publishedOn}</span>
            </li>
            <li className={`${styles.step} ${styles.stepEnd}`}>
              <span className={`${styles.dot} ${styles.dotEnd}`} aria-hidden>
                <CalendarCheck size={14} />
              </span>
              <span className={styles.stepLabel}>Encerrada em</span>
              <span className={styles.stepValue}>{closedOn}</span>
            </li>
          </ol>
        ) : null}
        {openPeriod ? (
          <p className={styles.chip}>
            <Hourglass size={14} aria-hidden />
            {openPeriod}
          </p>
        ) : null}

        <Button asChild variant="primary" className={styles.cta}>
          <Link href="/vagas">
            Ver vagas abertas
            <ArrowRight size={18} aria-hidden className={styles.arrow} />
          </Link>
        </Button>
      </div>
    </aside>
  );
}
