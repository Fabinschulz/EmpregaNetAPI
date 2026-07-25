'use client';

import { Button } from '@/components/ui';
import { PanelLeft, PanelLeftClose } from 'lucide-react';
import Link from 'next/link';
import styles from './sidebar.module.scss';

type SidebarFooterProps = {
  collapsed: boolean;
  isAuthenticated: boolean;
  onToggleCollapsed: () => void;
  onLogout: () => void;
};

export function SidebarFooter({ collapsed, isAuthenticated, onToggleCollapsed, onLogout }: SidebarFooterProps) {
  return (
    <div className={styles.footer}>
      <div className={styles.footerRailSlot}>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className={styles.desktopRailToggle}
          aria-label={collapsed ? 'Expandir menu lateral' : 'Compactar menu lateral'}
          onClick={onToggleCollapsed}
        >
          {collapsed ? (
            <PanelLeft className={styles.headerIcon} aria-hidden />
          ) : (
            <PanelLeftClose className={styles.headerIcon} aria-hidden />
          )}
        </Button>
      </div>
      {isAuthenticated ? (
        <Button variant="outline" size="sm" className={styles.logoutBtn} onClick={onLogout}>
          Sair
        </Button>
      ) : (
        <Button variant="primary" size="sm" className={styles.logoutBtn} asChild>
          <Link href="/login">Entrar</Link>
        </Button>
      )}
    </div>
  );
}
