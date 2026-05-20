'use client';

import { TooltipProvider } from '@/components';
import { ThemeProvider, useTheme } from '@/context';
import { QueryProvider } from '@/utils';
import type { ReactNode } from 'react';
import { Toaster } from 'sonner';

type AppProvidersProps = {
  children: ReactNode;
};

function ThemedToaster() {
  const { resolvedTheme } = useTheme();
  const sonnerTheme = resolvedTheme === 'dark' ? 'dark' : 'light';
  return (
    <Toaster
      position="top-right"
      theme={sonnerTheme}
      richColors
      closeButton
      expand={false}
      gap={10}
      toastOptions={{
        duration: 5000,
        classNames: {
          toast: 'toast',
          title: 'toast-title',
          description: 'toast-description'
        }
      }}
    />
  );
}

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <QueryProvider>
      <ThemeProvider>
        <TooltipProvider delayDuration={280}>
          {children}
          <ThemedToaster />
        </TooltipProvider>
      </ThemeProvider>
    </QueryProvider>
  );
}
