'use client';

import { useEffect, useState } from 'react';

/** Evita mismatch de hidratação ao ler tema/DOM só disponível no cliente (ex.: next-themes). */
export function useHasMounted(): boolean {
  const [mounted, setMounted] = useState(false);
  useEffect(() => {
    setMounted(true);
  }, []);
  return mounted;
}
