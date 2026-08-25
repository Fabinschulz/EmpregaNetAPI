'use client';

import { actionIcons, IconButton } from '@/shared/components';
import Link from 'next/link';
import styles from './sidebar.module.scss';

type SidebarHeaderProps = {
  onNavigate: () => void;
  onCloseMobile: () => void;
};

export function SidebarHeader({ onNavigate, onCloseMobile }: SidebarHeaderProps) {
  return (
    <div className={styles.header}>
      <Link href="/dashboard" className={styles.brand} onClick={onNavigate}>
        EmpregaUAI
      </Link>
      <div className={styles.headerActions}>
        <IconButton
          icon={actionIcons.close}
          label="Fechar menu"
          className={styles.mobileClose}
          iconStyleOverrides={styles.headerIcon}
          showTooltip={false}
          onClick={onCloseMobile}
        />
      </div>
    </div>
  );
}
