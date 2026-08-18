import { ResetPassword } from '@/features/auth';
import { LoadingState } from '@/shared/components';
import { Metadata } from 'next';
import { Suspense } from 'react';

export const metadata: Metadata = { title: 'Redefinir Senha' };
export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ResetPassword />
    </Suspense>
  );
}
