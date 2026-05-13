'use client';

import type { ReactNode } from 'react';
import { AuthProvider } from '@/context';

type MainSessionBoundaryProps = {
  children: ReactNode;
};

/** Sessão da área autenticada `(main)` — isolado do chrome (`AppShell`). */
export function MainSessionBoundary({ children }: MainSessionBoundaryProps) {
  return <AuthProvider>{children}</AuthProvider>;
}
