'use client';

import { actionIcons } from '@/shared/components';
import styles from './applied-badge.module.scss';

export function AppliedBadge() {
  return (
    <span className={styles.appliedBadge}>
      <actionIcons.confirm className={styles.appliedIcon} aria-hidden />
      Candidatura enviada
    </span>
  );
}
