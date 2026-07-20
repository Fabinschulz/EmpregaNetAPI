import { Suspense } from 'react';
import { MainLayout } from '@/shared/shell';

function MainChromeFallback() {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'var(--muted)',
        fontFamily: 'var(--font-sans)'
      }}
    >
      A carregar…
    </div>
  );
}

export default function MainSegmentLayout({ children }: { children: React.ReactNode }) {
  return (
    <Suspense fallback={<MainChromeFallback />}>
      <MainLayout>{children}</MainLayout>
    </Suspense>
  );
}
