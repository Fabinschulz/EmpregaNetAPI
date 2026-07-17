'use client';

import { useEffect, useState } from 'react';

/**
 * Retorna uma versão "atrasada" de `value`: só atualiza depois de `delayMs` sem novas mudanças.
 * Útil em campos de busca para evitar disparar uma request a cada tecla.
 */
export function useDebouncedValue<T>(value: T, delayMs = 350): T {
  const [debounced, setDebounced] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs);
    //cleanup função que cancela o timer se o componente for desmontado ou se o value mudar antes do delay
    return () => clearTimeout(timer);
  }, [value, delayMs]);

  return debounced;
}
