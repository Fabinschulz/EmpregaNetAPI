import { AuthSessionChecking } from '@/components';
import { Unauthorized } from '@/shared/status';
import { Suspense } from 'react';

export default function UnauthorizedPage() {
  return (
    <Suspense fallback={<AuthSessionChecking message="A carregar…" />}>
      <Unauthorized />
    </Suspense>
  );
}
