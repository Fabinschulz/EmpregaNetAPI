'use client';

import type { ReactNode } from 'react';
import { Toaster } from 'sonner';
import { TooltipProvider } from '@/components/ui';

type AppProvidersProps = {
  children: ReactNode;
};

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <TooltipProvider delayDuration={280}>
      {children}
      <Toaster
        position="top-right"
        theme="dark"
        richColors
        closeButton
        expand={false}
        gap={10}
        toastOptions={{
          duration: 5000,
          classNames: {
            toast: 'empreganet-toast',
            title: 'empreganet-toast-title',
            description: 'empreganet-toast-description'
          }
        }}
      />
    </TooltipProvider>
  );
}
