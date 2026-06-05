'use client';

import { Button } from '@/components/ui';
import clsx from 'clsx';
import Link from 'next/link';
import type { ShellNavItem } from '../hooks/use-app-shell-navigation';
import styles from './sidebar.module.scss';

type SidebarNavItemProps = {
  item: ShellNavItem;
  active: boolean;
  collapsed: boolean;
  onNavigate: () => void;
};

export function SidebarNavItem({ item, active, collapsed, onNavigate }: SidebarNavItemProps) {
  const Icon = item.icon;

  return (
    <Button
      asChild
      variant="ghost"
      className={clsx(styles.navButton, styles.link, active && styles.active, collapsed && styles.linkIconOnly)}
    >
      <Link href={item.href} onClick={onNavigate} title={collapsed ? item.label : undefined}>
        <Icon className={styles.navIcon} aria-hidden />
        {!collapsed ? <span className={styles.navLabel}>{item.label}</span> : null}
      </Link>
    </Button>
  );
}
