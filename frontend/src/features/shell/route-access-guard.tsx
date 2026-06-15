'use client';

import { useAuth } from '@/context';
import { AuthSessionChecking } from '@/features/auth/shared';
import { buildForbiddenRedirectPath, buildLoginRedirectPath, evaluateRouteAccess } from '@/utils';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect, type ReactNode } from 'react';

type RouteAccessGuardProps = {
  children: ReactNode;
};

export function RouteAccessGuard({ children }: RouteAccessGuardProps) {
  const { token, roles, hydrated } = useAuth();
  const pathname = usePathname();
  const router = useRouter();

  const session = { token, roles };
  const decision = evaluateRouteAccess(pathname, session);

  useEffect(() => {
    if (!hydrated) return;

    if (decision === 'login') {
      router.replace(buildLoginRedirectPath(pathname));
      return;
    }

    if (decision === 'forbidden') {
      router.replace(buildForbiddenRedirectPath(pathname));
    }
  }, [hydrated, decision, pathname, router]);

  if (!hydrated) return <AuthSessionChecking />;
  if (decision !== 'allow') return null;

  return <>{children}</>;
}
