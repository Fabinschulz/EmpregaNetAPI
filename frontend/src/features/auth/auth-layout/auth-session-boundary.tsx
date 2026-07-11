'use client';

import { useAuth } from '@/context';
import { AuthSessionChecking } from '@/features/auth/shared';
import { me } from '@/services';
import { resolvePostLoginPath } from '@/utils';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect, useRef, useState, type ReactNode } from 'react';

type AuthSessionBoundaryProps = {
  children: ReactNode;
};

type ServerSessionStatus = 'checking' | 'confirmed' | 'invalid';

/** Redireciona utilizadores já autenticados para fora das páginas de auth. */
export function AuthSessionBoundary({ children }: AuthSessionBoundaryProps) {
  const { isAuthenticated, hydrated } = useAuth();
  const router = useRouter();
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
    router.replace(resolvePostLoginPath(searchParams));
  }, [hydrated, isAuthenticated, serverSession, router, searchParams]);

  if (!hydrated) {
    return <AuthSessionChecking />;
  }

  if (isAuthenticated) {
    if (serverSession === 'confirmed') return null;
    if (serverSession === 'checking') return <AuthSessionChecking />;
  }

  return <>{children}</>;
}
