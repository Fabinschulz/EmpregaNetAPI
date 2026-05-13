'use client';

import type { ReactNode } from 'react';
import { AuthProvider } from '@/context';

type AuthSessionBoundaryProps = {
  children: ReactNode;
};

export function AuthSessionBoundary({ children }: AuthSessionBoundaryProps) {
  return <AuthProvider>{children}</AuthProvider>;
}
