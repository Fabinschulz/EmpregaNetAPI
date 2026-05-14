import { Skeleton } from '@/components/ui';
import clsx from 'clsx';
import styles from './form-fields-skeleton.module.scss';

type FormFieldsSkeletonProps = {
  fields?: number;
};

/**
 * Skeleton genérico para blocos de formulário.
 */
export function FormFieldsSkeleton({ fields = 5 }: FormFieldsSkeletonProps) {
  return (
    <div className={styles.wrap} role="status" aria-live="polite" aria-busy="true">
      <span className="sr-only">A carregar conteúdo…</span>
      {Array.from({ length: fields }, (_, i) => (
        <Skeleton key={i} className={clsx(styles.field, i === 0 ? styles.mid : styles.wide)} />
      ))}
    </div>
  );
}
