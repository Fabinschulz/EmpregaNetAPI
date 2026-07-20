'use client';

import type { ReactNode } from 'react';
import { AppShell } from '../AppShell';
import { RouteAccessGuard } from '../route-access-guard';

type MainLayoutProps = {
  children: ReactNode;
};

/**
 * Layout principal do aplicativo, que envolve o conteúdo com a estrutura de navegação e proteção de rotas.
 */
export function MainLayout({ children }: MainLayoutProps) {
  return (
    <RouteAccessGuard>
      <AppShell>{children}</AppShell>
    </RouteAccessGuard>
  );
}
