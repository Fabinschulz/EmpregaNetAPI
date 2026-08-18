'use client';

import { TooltipProvider } from '@/shared/components';
import { AuthProvider } from '@/shared/context';
import { QueryProvider } from '@/shared/utils';
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
