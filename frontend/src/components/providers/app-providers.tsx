'use client';

import { TooltipProvider } from '@/components/ui';
import type { ReactNode } from 'react';
import { Toaster } from 'sonner';
import { ThemeProvider, useTheme } from '@/context';

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
    <ThemeProvider>
      <TooltipProvider delayDuration={280}>
        {children}
        <ThemedToaster />
      </TooltipProvider>
    </ThemeProvider>
  );
}
