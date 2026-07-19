'use client';

import { TooltipProvider } from '@/components';
import { AuthProvider } from '@/context';
import { QueryProvider } from '@/utils';
import type { ReactNode } from 'react';
import { ThemedToaster, ThemeProvider } from './theme-provider';

type AppProvidersProps = {
  children: ReactNode;
};

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <QueryProvider>
      <AuthProvider>
        <ThemeProvider>
          <TooltipProvider delayDuration={280}>
            {children}
            <ThemedToaster />
          </TooltipProvider>
        </ThemeProvider>
      </AuthProvider>
    </QueryProvider>
  );
}
