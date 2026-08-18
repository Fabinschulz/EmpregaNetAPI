'use client';

import type { JobFeedItemResponse } from '@/features/vagas/service';
import { useRelativeTime } from '@/shared/hooks';
import { jobAreaVocabulary } from '@/shared/schema';
import { cn, formatSalaryRange } from '@/shared/utils';
import { MapPin, Users } from 'lucide-react';
import Link from 'next/link';
import { JobCardActions } from './actions';
import { CompanyAvatar } from './company-avatar';
import { JobCardHighlights } from './highlights';
import styles from './job-card.module.scss';
import { JobCardTags } from './tags';

/** Reexportado aqui para que a pasta tenha uma única entrada pública. */
export { JobCardSkeleton, JobCardSkeletonList } from './skeleton';

type JobCardProps = {
  job: JobFeedItemResponse;
  hasApplied?: boolean;
  position: number;
  totalItems: number;
  className?: string;
};

export function JobCard({ job, hasApplied, position, totalItems, className }: JobCardProps) {
  const publishedLabel = useRelativeTime(job.publishedAt);
  const titleId = `job-card-title-${job.id}`;

  const area = jobAreaVocabulary.normalize(job.area);
  const salary = formatSalaryRange(job.salary.min, job.salary.max, job.salary.disclosed);

  return (
    <article
      className={cn(styles.card, className)}
      aria-labelledby={titleId}
      aria-posinset={position}
      aria-setsize={totalItems}
    >
      <header className={styles.header}>
        <CompanyAvatar name={job.company.name} logoUrl={job.company.logoUrl} />
        <p className={styles.company}>{job.company.name}</p>
      </header>

      <h2 className={styles.title} id={titleId}>
        <Link href={`/vagas/${job.id}`} className={styles.titleLink}>
          {job.title}
        </Link>
      </h2>

      <p className={styles.facts}>
        <span className={styles.location}>
          <MapPin className={styles.chipIcon} aria-hidden />
          {job.location.city}, {job.location.state}
        </span>

        <span className={styles.factSeparator} aria-hidden>
          ·
        </span>

        <span className={styles.salary} data-disclosed={job.salary.disclosed}>
          {salary}
        </span>
      </p>

      <JobCardHighlights job={job} />

      <JobCardTags requirements={job.requirements} benefits={job.benefits} />

      <footer className={styles.footer}>
        <div className={styles.meta}>
          {area ? <span className={styles.area}>{jobAreaVocabulary.label(area)}</span> : null}

          {job.applicationsCount > 0 ? (
            <span className={styles.applicants}>
              <Users className={styles.chipIcon} aria-hidden />
              {job.applicationsCount} {job.applicationsCount === 1 ? 'candidato' : 'candidatos'}
            </span>
          ) : null}

          <time className={styles.published} dateTime={job.publishedAt}>
            {publishedLabel}
          </time>
        </div>

        <JobCardActions jobId={job.id} jobTitle={job.title} hasApplied={hasApplied} />
      </footer>
    </article>
  );
}
