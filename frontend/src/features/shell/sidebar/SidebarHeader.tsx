'use client';

import { Button } from '@/components/ui';
import { X } from 'lucide-react';
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
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className={styles.mobileClose}
          aria-label="Fechar menu"
          onClick={onCloseMobile}
        >
          <X className={styles.headerIcon} aria-hidden />
        </Button>
      </div>
    </div>
  );
}
