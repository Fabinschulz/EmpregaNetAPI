import { EntityAvatarSkeleton, Skeleton } from '@/shared/components';
import styles from './job-card.module.scss';

export function JobCardSkeleton() {
  return (
    <div className={styles.card} aria-hidden>
      <div className={styles.skeletonIdentity}>
        <EntityAvatarSkeleton />

        <div className={styles.skeletonIdentityText}>
          <Skeleton className={styles.skeletonLine} style={{ height: 12, width: '26%' }} />
          <Skeleton className={styles.skeletonLine} style={{ height: 18, width: '58%' }} />
        </div>
      </div>

      <div className={styles.skeletonRow}>
        <Skeleton className={styles.skeletonLine} style={{ height: 14, width: 112 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 14, width: 96 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 14, width: 84 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 14, width: 70 }} />
      </div>

      <Skeleton className={styles.skeletonLine} style={{ height: 13, width: '92%' }} />
      <Skeleton className={styles.skeletonLine} style={{ height: 13, width: '64%' }} />

      <div className={styles.skeletonRow}>
        <Skeleton className={styles.skeletonLine} style={{ height: 20, width: 88, borderRadius: 999 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 20, width: 64, borderRadius: 999 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 20, width: 104, borderRadius: 999 }} />
      </div>

      <div className={styles.skeletonFooter}>
        <Skeleton className={styles.skeletonLine} style={{ height: 13, width: 148 }} />
        <Skeleton className={styles.skeletonLine} style={{ height: 36, width: 220, borderRadius: 999 }} />
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
