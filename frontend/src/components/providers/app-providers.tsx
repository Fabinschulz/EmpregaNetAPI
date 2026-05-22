'use client';

import { TooltipProvider } from '@/components';
import { ThemeProvider } from '@/context';
import { QueryProvider } from '@/utils';
import type { ReactNode } from 'react';
import { ThemedToaster } from './theme-provider';

type AppProvidersProps = {
  children: ReactNode;
};

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
