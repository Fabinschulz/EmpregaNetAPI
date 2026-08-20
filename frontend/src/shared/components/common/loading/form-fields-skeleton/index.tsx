import { Skeleton } from '@/shared/components';
import styles from './form-fields-skeleton.module.scss';

type FormFieldsSkeletonProps = {
  fields?: number;
};

export function FormFieldsSkeleton({ fields = 5 }: FormFieldsSkeletonProps) {
  return (
    <div className={styles.wrap} role="status" aria-live="polite" aria-busy="true">
      <span className="sr-only">Carregando conteúdo…</span>
      {Array.from({ length: fields }, (_, i) => (
        <div key={i} className={styles.field}>
          <Skeleton className={styles.label} />
          <Skeleton className={styles.control} />
        </div>
      ))}
    </div>
  );
}
