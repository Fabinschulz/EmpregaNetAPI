import type { JobFeedItemResponse } from '@/features/vagas/service';
import { EntityAvatar, StatusBadge } from '@/shared/components';
import Link from 'next/link';
import { publicJobsRoutes } from '../../public-jobs-routes';
import styles from './job-card.module.scss';

type JobCardIdentityProps = {
  job: JobFeedItemResponse;
  titleId: string;
};

export function JobCardIdentity({ job, titleId }: JobCardIdentityProps) {
  return (
    <header className={styles.identity}>
      <EntityAvatar name={job.company.name} imageUrl={job.company.logoUrl} size="md" />

      <div className={styles.identityText}>
        <p className={styles.company} title={job.company.name}>
          {job.company.name}
        </p>

        <h2 className={styles.title} id={titleId}>
          <Link href={publicJobsRoutes.detail(job.id)} className={styles.titleLink}>
            {job.title}
          </Link>
        </h2>
      </div>

      {job.isActive ? null : (
        <div className={styles.identityAside}>
          <StatusBadge label="Encerrada" tone="negative" />
        </div>
      )}
    </header>
  );
}
