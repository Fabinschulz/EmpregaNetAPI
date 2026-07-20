'use client';

import { Button } from '@/components';
import clsx from 'clsx';
import { ChevronDown } from 'lucide-react';
import Link from 'next/link';
import { useId, useState } from 'react';
import type { ShellNavItem } from '../hooks/use-app-shell-navigation';
import styles from './sidebar.module.scss';

type SidebarNavSubmenuProps = {
  item: ShellNavItem;
  /** Rota atual usada para destacar o subitem ativo e auto-expandir a seção. */
  pathname: string | null;
  onNavigate: () => void;
};

function isPathActive(pathname: string | null, href: string): boolean {
  return pathname === href || Boolean(pathname?.startsWith(`${href}/`));
}

/**
 * Item de navegação com submenu: botão que expande/recolhe
 * e subitens como links com `aria-current`. Auto-expande quando a rota atual pertence à seção.
 */
export function SidebarNavSubmenu({ item, pathname, onNavigate }: SidebarNavSubmenuProps) {
  const Icon = item.icon;
  const submenuId = useId();
  const visibleChildren = (item.children ?? []).filter((child) => child.visible);
  const hasActiveChild = visibleChildren.some((child) => isPathActive(pathname, child.href));
  const [open, setOpen] = useState(hasActiveChild);
  const [wasActive, setWasActive] = useState(hasActiveChild);

  // OBS: Entrar na seção (por navegação externa ao submenu) reabre o grupo, ajuste feito
  // durante a renderização (padrão React), sem useEffect nem re-render extra.
  if (hasActiveChild !== wasActive) {
    setWasActive(hasActiveChild);
    if (hasActiveChild) setOpen(true);
  }

  return (
    <div>
      <Button
        type="button"
        variant="ghost"
        className={clsx(styles.navButton, styles.link, hasActiveChild && !open && styles.active)}
        onClick={() => setOpen((current) => !current)}
        aria-expanded={open}
        aria-controls={submenuId}
      >
        <Icon className={styles.navIcon} aria-hidden />
        <span className={styles.navLabel}>{item.label}</span>
        <ChevronDown className={clsx(styles.submenuChevron, open && styles.submenuChevronOpen)} aria-hidden />
      </Button>

      {open ? (
        <div id={submenuId} className={styles.subList} role="group" aria-label={item.label}>
          {visibleChildren.map((child) => {
            const active = isPathActive(pathname, child.href);
            return (
              <Button
                key={child.href}
                asChild
                variant="ghost"
                className={clsx(styles.navButton, styles.link, styles.subLink, active && styles.active)}
              >
                <Link href={child.href} onClick={onNavigate} aria-current={active ? 'page' : undefined}>
                  <span className={styles.navLabel}>{child.label}</span>
                </Link>
              </Button>
            );
          })}
        </div>
      ) : null}
    </div>
  );
}
