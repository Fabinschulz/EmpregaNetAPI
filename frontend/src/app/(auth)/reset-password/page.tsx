import { LoadingState } from '@/components';
import { ResetPassword } from '@/features/auth/reset-password';
import { Suspense } from 'react';

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ResetPassword />
    </Suspense>
  );
}
