'use client';

import * as Dialog from '@radix-ui/react-dialog';
import clsx from 'clsx';
import type { ShellNavGroup } from './hooks/use-app-shell-navigation';
import { SidebarFooter } from './sidebar/SidebarFooter';
import { SidebarHeader } from './sidebar/SidebarHeader';
import { SidebarNav } from './sidebar/SidebarNav';
import shell from './AppShell.module.scss';
import sidebar from './sidebar/sidebar.module.scss';

type MobileNavProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  groups: ShellNavGroup[];
  onNavigate: () => void;
  onLogout: () => void;
};

/**
 * Drawer de navegação no mobile como dialog acessível (Radix): `role=dialog`,
 * `aria-modal`, focus-trap, fecha no Escape e no clique fora, e devolve o foco
 * ao botão que o abriu. Reaproveita o visual do sidebar existente.
 */
export function MobileNav({ open, onOpenChange, groups, onNavigate, onLogout }: MobileNavProps) {
  const close = () => onOpenChange(false);

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className={shell.mobileOverlay} />
        <Dialog.Content className={clsx(sidebar.root, sidebar.mobileOpen)} aria-describedby={undefined}>
          <Dialog.Title className="sr-only">Menu de navegação</Dialog.Title>
          <SidebarHeader onNavigate={onNavigate} onCloseMobile={close} />
          <SidebarNav
            id="mobile-sidebar-nav"
            groups={groups}
            collapsed={false}
            transitioning={false}
            onNavigate={onNavigate}
          />
          <SidebarFooter collapsed={false} onToggleCollapsed={close} onLogout={onLogout} />
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
