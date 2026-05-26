import { ResetPassword } from '@/features/auth/reset-password';
import { Suspense } from 'react';

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<p style={{ textAlign: 'center', color: 'var(--muted)' }}>Carregando…</p>}>
      <ResetPassword />
    </Suspense>
  );
}
