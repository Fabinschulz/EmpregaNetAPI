'use client';

import { actionIcons, Button, IconButton } from '@/shared/components';
import Link from 'next/link';
import styles from './sidebar.module.scss';

type SidebarFooterProps = {
  collapsed: boolean;
  isAuthenticated: boolean;
  onToggleCollapsed: () => void;
  onLogout: () => void;
};

export function SidebarFooter({ collapsed, isAuthenticated, onToggleCollapsed, onLogout }: SidebarFooterProps) {
  const SignOutIcon = collapsed ? undefined : actionIcons.signOut;

  return (
    <div className={styles.footer}>
      <div className={styles.footerRailSlot}>
        <IconButton
          icon={collapsed ? actionIcons.expandSidebar : actionIcons.collapseSidebar}
          label={collapsed ? 'Expandir menu lateral' : 'Compactar menu lateral'}
          className={styles.desktopRailToggle}
          iconStyleOverrides={styles.headerIcon}
          onClick={onToggleCollapsed}
        />
      </div>
      {isAuthenticated ? (
        <Button variant="outline" size="sm" className={styles.logoutBtn} startIcon={SignOutIcon} onClick={onLogout}>
          Sair
        </Button>
      ) : (
        <Button variant="primary" size="sm" className={styles.logoutBtn} asChild>
          <Link href="/login">
            {collapsed ? null : <actionIcons.signIn aria-hidden />}
            Entrar
          </Link>
        </Button>
      )}
    </div>
  );
}
