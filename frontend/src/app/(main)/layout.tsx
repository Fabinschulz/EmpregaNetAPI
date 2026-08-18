import { LoadingState } from '@/shared/components';
import { MainLayout } from '@/shared/shell';
import { Suspense } from 'react';

export default function MainSegmentLayout({ children }: { children: React.ReactNode }) {
  return (
    <Suspense fallback={<LoadingState fullscreen />}>
      <MainLayout>{children}</MainLayout>
    </Suspense>
  );
}
