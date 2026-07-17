'use client';

import clsx from 'clsx';
import { usePathname } from 'next/navigation';
import type { ShellNavGroup } from '../hooks/use-app-shell-navigation';
import { SidebarNavItem } from './SidebarNavItem';
import { SidebarNavSubmenu } from './SidebarNavSubmenu';
import styles from './sidebar.module.scss';

type SidebarNavProps = {
  id?: string;
  groups: ShellNavGroup[];
  collapsed: boolean;
  transitioning: boolean;
  onNavigate: () => void;
};

export function SidebarNav({ id = 'app-sidebar-nav', groups, collapsed, transitioning, onNavigate }: SidebarNavProps) {
  const pathname = usePathname();

  return (
    <nav
      id={id}
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
                const hasSubmenu = (item.children ?? []).some((child) => child.visible);

                if (hasSubmenu && !collapsed) {
                  return <SidebarNavSubmenu key={item.href} item={item} pathname={pathname} onNavigate={onNavigate} />;
                }

                const childActive = (item.children ?? []).some(
                  (child) => pathname === child.href || pathname?.startsWith(`${child.href}/`)
                );

                const active = childActive || pathname === item.href || pathname?.startsWith(`${item.href}/`);

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
