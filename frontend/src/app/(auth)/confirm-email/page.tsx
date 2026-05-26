import { ConfirmEmail } from '@/features/auth/confirm-email';
import { Suspense } from 'react';

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<p style={{ textAlign: 'center', color: 'var(--muted)' }}>Carregando…</p>}>
      <ConfirmEmail />
    </Suspense>
  );
}
