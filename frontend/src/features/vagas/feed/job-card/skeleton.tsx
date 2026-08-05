import { Skeleton } from '@/components';
import styles from './job-card.module.scss';

export function JobCardSkeleton() {
  return (
    <div className={styles.card} aria-hidden>
      <div className={styles.skeletonHeader}>
        <Skeleton className={styles.avatar} />
        <Skeleton className={styles.skeletonLine} style={{ height: 12, width: '28%' }} />
      </div>

      <Skeleton className={styles.skeletonLine} style={{ height: 19, width: '62%' }} />
      <Skeleton className={styles.skeletonLine} style={{ height: 14, width: '44%' }} />

      <div className={styles.skeletonChips}>
        <Skeleton className={styles.skeletonLine} style={{ height: 22, width: 92 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 22, width: 68 }} />
      </div>

      <div className={styles.skeletonChips}>
        <Skeleton className={styles.skeletonLine} style={{ height: 20, width: 76 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 20, width: 58 }} />
      </div>
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
