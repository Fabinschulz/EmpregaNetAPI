'use client';

import { Suspense, type ReactNode } from 'react';
import { AuthLayoutFrame } from './auth-layout-frame';
import { AuthSessionBoundary } from './auth-session-boundary';

type AuthLayoutProps = {
  children: ReactNode;
};

export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <AuthLayoutFrame>
      <Suspense fallback={null}>
        <AuthSessionBoundary>{children}</AuthSessionBoundary>
      </Suspense>
    </AuthLayoutFrame>
  );
}
