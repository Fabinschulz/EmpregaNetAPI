'use client';

import { useAuth } from '@/context';
import { AuthSessionChecking } from '@/features/auth/shared';
import { resolvePostLoginPath } from '@/utils';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect, type ReactNode } from 'react';

type AuthSessionBoundaryProps = {
  children: ReactNode;
};

/** Redireciona utilizadores já autenticados para fora das páginas de auth. */
export function AuthSessionBoundary({ children }: AuthSessionBoundaryProps) {
  const { isAuthenticated, hydrated } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    if (!hydrated || !isAuthenticated) return;
    router.replace(resolvePostLoginPath(searchParams));
  }, [hydrated, isAuthenticated, router, searchParams]);

  if (!hydrated) {
    return <AuthSessionChecking />;
  }

  if (isAuthenticated) return null;

  return <>{children}</>;
}
