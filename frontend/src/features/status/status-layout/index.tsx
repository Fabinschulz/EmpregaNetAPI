'use client';

import { CenteredPageFrame } from '@/components';
import type { ReactNode } from 'react';

type StatusLayoutProps = {
  children: ReactNode;
};

/**
 * Layout para páginas de estado (ex.: não autorizado) — mesmo chrome das rotas de auth,
 * sem redirecionar utilizadores já autenticados.
 */
export function StatusLayout({ children }: StatusLayoutProps) {
  return <CenteredPageFrame>{children}</CenteredPageFrame>;
}
