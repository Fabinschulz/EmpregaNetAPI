'use client';

import { useAuth } from '@/context';
import { canAccessPath } from '@/utils/lib/rbac';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect, type ReactNode } from 'react';

type RouteAccessGuardProps = {
  children: ReactNode;
};

export function RouteAccessGuard({ children }: RouteAccessGuardProps) {
  const { token, roles, hydrated, logout } = useAuth();
  const pathname = usePathname();
  const router = useRouter();

  useEffect(() => {
    if (!hydrated) return;

    if (!token) {
      router.replace(`/login?next=${encodeURIComponent(pathname)}`);
      return;
    }

    if (!canAccessPath(pathname, roles)) {
      router.replace('/dashboard');
    }
  }, [hydrated, token, roles, pathname, router, logout]);

  if (!hydrated) {
    return (
      <div
        style={{
          minHeight: '40vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--muted)'
        }}
      >
        A verificar sessão…
      </div>
    );
  }

  if (!token || !canAccessPath(pathname, roles)) {
    return null;
  }

  return <>{children}</>;
}
