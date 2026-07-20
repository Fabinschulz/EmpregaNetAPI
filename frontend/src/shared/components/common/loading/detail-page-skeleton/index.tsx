import { Skeleton } from '@/components';
import styles from './detail-page-skeleton.module.scss';

type DetailPageSkeletonProps = {
  /** Linhas de texto simuladas após o título. */
  bodyLines?: number;
};

/**
 * Skeleton para páginas de detalhe (título + parágrafos + ações).
 */
export function DetailPageSkeleton({ bodyLines = 4 }: DetailPageSkeletonProps) {
  return (
    <div className={styles.wrap} role="status" aria-live="polite" aria-busy="true">
      <span className="sr-only">A carregar conteúdo…</span>
      <Skeleton className={styles.title} />
      {Array.from({ length: bodyLines }, (_, i) => (
        <Skeleton key={i} className={i === bodyLines - 1 ? styles.lineShort : styles.line} />
      ))}
      <div className={styles.actions}>
        <Skeleton className={styles.btn} />
      </div>
    </div>
  );
}
