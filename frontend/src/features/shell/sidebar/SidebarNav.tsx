'use client';

import clsx from 'clsx';
import { usePathname } from 'next/navigation';
import type { ShellNavGroup } from '../hooks/use-app-shell-navigation';
import { SidebarNavItem } from './SidebarNavItem';
import styles from './sidebar.module.scss';

type SidebarNavProps = {
  groups: ShellNavGroup[];
  collapsed: boolean;
  transitioning: boolean;
  onNavigate: () => void;
};

export function SidebarNav({ groups, collapsed, transitioning, onNavigate }: SidebarNavProps) {
  const pathname = usePathname();

  return (
    <nav
      id="app-sidebar-nav"
      className={clsx(styles.scroll, transitioning && styles.scrollTransitioning)}
      aria-label="Navegação principal"
      suppressHydrationWarning
    >
      {groups.map((group) => {
        const visibleItems = group.items.filter((item) => item.visible);
        if (visibleItems.length === 0) return null;

        return (
          <div key={group.id} className={styles.navGroup}>
            {!collapsed ? <div className={styles.navGroupTitle}>{group.title}</div> : null}
            <div className={styles.navList}>
              {visibleItems.map((item) => {
                const active = pathname === item.href || pathname?.startsWith(`${item.href}/`);

                return (
                  <SidebarNavItem
                    key={item.href}
                    item={item}
                    active={Boolean(active)}
                    collapsed={collapsed}
                    onNavigate={onNavigate}
                  />
                );
              })}
            </div>
          </div>
        );
      })}
    </nav>
  );
}
