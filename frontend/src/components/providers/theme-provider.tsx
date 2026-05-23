'use client';

import { ThemeProvider as NextThemesProvider, useTheme } from 'next-themes';
import type { ComponentProps } from 'react';
import { Toaster } from 'sonner';

type NextThemeProviderProps = ComponentProps<typeof NextThemesProvider>;

export const THEME_STORAGE_KEY = 'key-theme';

/**
 * Tema global via next-themes: `data-theme` em `html` (alinhado a globals.scss).
 */
export function ThemeProvider({ children, ...props }: NextThemeProviderProps) {
  return (
    <NextThemesProvider
      attribute="data-theme"
      defaultTheme="system"
      enableSystem
      enableColorScheme
      disableTransitionOnChange
      storageKey={THEME_STORAGE_KEY}
      {...props}
    >
      {children}
    </NextThemesProvider>
  );
}

export function ThemedToaster() {
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
