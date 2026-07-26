import { LoadingState } from '@/components';
import { Unauthorized } from '@/shared/status';
import { Suspense } from 'react';

export default function UnauthorizedPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <Unauthorized />
    </Suspense>
  );
}
