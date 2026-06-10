import { AuthSessionChecking } from '@/features/auth/shared';
import { Unauthorized } from '@/features/status';
import { Suspense } from 'react';

export default function UnauthorizedPage() {
  return (
    <Suspense fallback={<AuthSessionChecking message="A carregar…" />}>
      <Unauthorized />
    </Suspense>
  );
}
