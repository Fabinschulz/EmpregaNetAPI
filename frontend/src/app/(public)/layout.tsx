import { AppShell } from '@/shared/shell';
import { Suspense } from 'react';

function PublicContentFallback() {
  return (
    <div
      style={{
        minHeight: '40vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'var(--muted)',
        fontFamily: 'var(--font-sans)'
      }}
      role="status"
    >
      Carregando...
    </div>
  );
}

export default function PublicSegmentLayout({ children }: { children: React.ReactNode }) {

  return (
    <Suspense fallback={<PublicContentFallback />}>
      <AppShell>{children}</AppShell>
    </Suspense>
  );
}
