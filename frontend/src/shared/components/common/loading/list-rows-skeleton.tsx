import { Skeleton } from '@/components';
import styles from './list-rows-skeleton.module.scss';

type ListRowsSkeletonProps = {
  rows?: number;
};

/**
 * Skeleton para listas tipo “vaga / candidatura / utilizador”.
 */
export function ListRowsSkeleton({ rows = 5 }: ListRowsSkeletonProps) {
  return (
    <div className={styles.wrap} role="status" aria-live="polite" aria-busy="true">
      <span className="sr-only">A carregar conteúdo…</span>
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className={styles.row}>
          <div className={styles.col}>
            <Skeleton className={styles.lineLg} />
            <Skeleton className={styles.lineSm} />
          </div>
          <Skeleton className={styles.btn} />
        </div>
      ))}
    </div>
  );
}
