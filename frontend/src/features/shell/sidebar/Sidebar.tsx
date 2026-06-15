'use client';

import clsx from 'clsx';
import type { ShellNavGroup } from '../hooks/use-app-shell-navigation';
import { SidebarFooter } from './SidebarFooter';
import { SidebarHeader } from './SidebarHeader';
import { SidebarNav } from './SidebarNav';
import styles from './sidebar.module.scss';

type SidebarProps = {
  id?: string;
  groups: ShellNavGroup[];
  collapsed: boolean;
  mobileOpen: boolean;
  railTransitioning: boolean;
  onNavigate: () => void;
  onCloseMobile: () => void;
  onToggleCollapsed: () => void;
  onLogout: () => void;
};

export function Sidebar({
  id = 'app-sidebar',
  groups,
  collapsed,
  mobileOpen,
  railTransitioning,
  onNavigate,
  onCloseMobile,
  onToggleCollapsed,
  onLogout
}: SidebarProps) {
  return (
    <aside
      id={id}
      className={clsx(styles.root, collapsed && styles.collapsed, mobileOpen && styles.mobileOpen)}
      suppressHydrationWarning
    >
      <SidebarHeader onNavigate={onNavigate} onCloseMobile={onCloseMobile} />
      <SidebarNav groups={groups} collapsed={collapsed} transitioning={railTransitioning} onNavigate={onNavigate} />
      <SidebarFooter collapsed={collapsed} onToggleCollapsed={onToggleCollapsed} onLogout={onLogout} />
    </aside>
  );
}
