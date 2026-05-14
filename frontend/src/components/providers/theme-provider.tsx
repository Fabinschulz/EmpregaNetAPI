'use client';

import { ThemeProvider as NextThemesProvider } from 'next-themes';
import type { ComponentProps } from 'react';

type NextThemeProviderProps = ComponentProps<typeof NextThemesProvider>;

export const THEME_STORAGE_KEY = 'empreganet-theme';

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
