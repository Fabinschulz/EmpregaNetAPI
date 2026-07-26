import { LoadingState } from '@/components';
import { AppShell } from '@/shared/shell';
import { Suspense } from 'react';

export default function PublicSegmentLayout({ children }: { children: React.ReactNode }) {

  return (
    <Suspense fallback={<LoadingState fullscreen />}>
      <AppShell>{children}</AppShell>
    </Suspense>
  );
}
