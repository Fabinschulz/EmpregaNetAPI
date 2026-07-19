'use client';

import { Suspense, type ReactNode } from 'react';
import { CenteredPageFrame } from '@/components';
import { AuthSessionBoundary } from './auth-session-boundary';

type AuthLayoutProps = {
  children: ReactNode;
};

export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <CenteredPageFrame>
      <Suspense fallback={null}>
        <AuthSessionBoundary>{children}</AuthSessionBoundary>
      </Suspense>
    </CenteredPageFrame>
  );
}
