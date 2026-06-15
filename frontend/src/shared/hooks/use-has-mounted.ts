'use client';

import { useSyncExternalStore } from 'react';

/** Evita mismatch de hidratação ao ler tema/DOM só disponível no cliente (ex.: next-themes). */
export function useHasMounted(): boolean {
  return useSyncExternalStore(
    () => () => {},
    () => true,
    () => false
  );
}
