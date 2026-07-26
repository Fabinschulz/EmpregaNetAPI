import { LoadingState } from '@/components';
import { ConfirmEmail } from '@/features/auth/confirm-email';
import { Suspense } from 'react';

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ConfirmEmail />
    </Suspense>
  );
}
