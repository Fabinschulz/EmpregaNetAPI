'use client';

import { AuthLayoutFrame } from '@/features/auth/auth-layout/auth-layout-frame';
import type { ReactNode } from 'react';

type StatusLayoutProps = {
  children: ReactNode;
};

/**
 * Layout para páginas de estado (ex.: não autorizado) — mesmo chrome das rotas de auth,
 * sem redirecionar utilizadores já autenticados.
 */
export function StatusLayout({ children }: StatusLayoutProps) {
  return <AuthLayoutFrame>{children}</AuthLayoutFrame>;
}
