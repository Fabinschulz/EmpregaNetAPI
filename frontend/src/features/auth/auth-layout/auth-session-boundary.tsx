'use client';

import { me } from '@/features/conta/service';
import { AuthSessionChecking } from '@/shared/components';
import { useAuth } from '@/shared/context';
import { navigateAfterSignIn, resolvePostLoginPath } from '@/shared/utils';
import { useSearchParams } from 'next/navigation';
import { useEffect, useRef, useState, type ReactNode } from 'react';

type AuthSessionBoundaryProps = {
  children: ReactNode;
};

type ServerSessionStatus = 'checking' | 'confirmed' | 'invalid';

/** Redireciona utilizadores já autenticados para fora das páginas de auth. */
export function AuthSessionBoundary({ children }: AuthSessionBoundaryProps) {
  const { isAuthenticated, hydrated } = useAuth();
  const searchParams = useSearchParams();
  const [serverSession, setServerSession] = useState<ServerSessionStatus>('checking');
  const sessionCheckStartedRef = useRef(false);

  useEffect(() => {
    if (!hydrated || !isAuthenticated || sessionCheckStartedRef.current) return;
    sessionCheckStartedRef.current = true;
    me()
      .then(() => setServerSession('confirmed'))
      .catch(() => setServerSession('invalid'));
  }, [hydrated, isAuthenticated]);

  useEffect(() => {
    if (!hydrated || !isAuthenticated || serverSession !== 'confirmed') return;
    navigateAfterSignIn(resolvePostLoginPath(searchParams));
  }, [hydrated, isAuthenticated, serverSession, searchParams]);

  if (!hydrated) {
    return <AuthSessionChecking />;
  }

  if (isAuthenticated) {
    if (serverSession !== 'invalid') return <AuthSessionChecking />;
  }

  return <>{children}</>;
}
