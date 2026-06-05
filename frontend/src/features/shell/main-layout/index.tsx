'use client';

import type { ReactNode } from 'react';
import { RouteAccessGuard } from '../route-access-guard';
import { MainAppFrame } from './main-app-frame';
import { MainSessionBoundary } from './main-session-boundary';

type MainLayoutProps = {
  children: ReactNode;
};

/**
 * Layout do segmento `(main)`: sessão + shell.
 * O `app/(main)/layout.tsx` só importa este componente.
 */
export function MainLayout({ children }: MainLayoutProps) {
  return (
    <MainSessionBoundary>
      <RouteAccessGuard>
        <MainAppFrame>{children}</MainAppFrame>
      </RouteAccessGuard>
    </MainSessionBoundary>
  );
}
