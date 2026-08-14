import { LoadingState } from '@/components';
import { ConfirmEmail } from '@/features/auth/confirm-email';
import { Metadata } from 'next';
import { Suspense } from 'react';

export const metadata: Metadata = { title: 'Confirmar Email' };
export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ConfirmEmail />
    </Suspense>
  );
}
