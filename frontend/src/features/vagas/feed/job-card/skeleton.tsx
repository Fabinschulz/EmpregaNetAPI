import { Skeleton } from '@/components';
import styles from './job-card.module.scss';

export function JobCardSkeleton() {
  return (
    <div className={styles.card} aria-hidden>
      <div className={styles.skeletonHeader}>
        <Skeleton className={styles.avatar} />
        <div className={styles.skeletonHeaderText}>
          <Skeleton className={styles.skeletonLine} style={{ height: 14, width: '38%' }} />
          <Skeleton className={styles.skeletonLine} style={{ height: 12, width: '24%' }} />
        </div>
      </div>

      <Skeleton className={styles.skeletonLine} style={{ height: 20, width: '70%' }} />
      <Skeleton className={styles.skeletonLine} style={{ height: 16, width: '32%' }} />

      <div className={styles.skeletonChips}>
        <Skeleton className={styles.skeletonLine} style={{ height: 24, width: 92 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 24, width: 68 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 24, width: 76 }} />
      </div>

      <Skeleton className={styles.skeletonLine} style={{ height: 14, width: '100%' }} />
      <Skeleton className={styles.skeletonLine} style={{ height: 14, width: '82%' }} />
    </div>
  );
}

export function JobCardSkeletonList({ count = 4 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }, (_, index) => (
        <JobCardSkeleton key={index} />
      ))}
    </>
  );
}
