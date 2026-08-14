import { LoadingState } from '@/components';
import { ResetPassword } from '@/features/auth/reset-password';
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
