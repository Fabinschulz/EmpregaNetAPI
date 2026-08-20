'use client';

import type { JobFeedItemResponse } from '@/features/vagas/service';
import { cn } from '@/shared/utils';
import { JobCardActions } from './actions';
import { JobCardFacts } from './facts';
import { JobCardIdentity } from './identity';
import styles from './job-card.module.scss';
import { JobCardStatus } from './status';
import { JobCardTags } from './tags';

type JobCardProps = {
  job: JobFeedItemResponse;
  hasApplied?: boolean;
  position: number;
  totalItems: number;
  className?: string;
};

export function JobCard({ job, hasApplied, position, totalItems, className }: JobCardProps) {
  const titleId = `job-card-title-${job.id}`;
  const summary = job.summary?.trim();

  return (
    <article
      className={cn(styles.card, className)}
      aria-labelledby={titleId}
      aria-posinset={position}
      aria-setsize={totalItems}
      data-closed={!job.isActive}
    >
      <JobCardIdentity job={job} titleId={titleId} />

      <JobCardFacts job={job} />

      {summary ? <p className={styles.summary}>{summary}</p> : null}

      <JobCardTags job={job} />

      <footer className={styles.footer}>
        <JobCardStatus publishedAt={job.publishedAt} applicationsCount={job.applicationsCount} />

        <JobCardActions
          jobId={job.id}
          jobTitle={job.title}
          hasApplied={hasApplied}
          isActive={job.isActive}
        />
      </footer>
    </article>
  );
}
