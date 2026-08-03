'use client';

import { formatDate, formatRelativeTime } from '@/utils';
import { useSyncExternalStore } from 'react';

/** Com que frequência o rótulo se atualiza. Um minuto basta para "há 3 minutos". */
const TICK_MS = 60_000;

/**
 * Relógio compartilhado por todos os consumidores do hook.
 *
 * Um `setInterval` por cartão daria 20+ timers numa página de feed, todos acordando no mesmo
 * segundo. Aqui é um só, criado no primeiro subscritor e destruído quando o último sai.
 */
let snapshot = 0;
const listeners = new Set<() => void>();
let timer: ReturnType<typeof setInterval> | null = null;

function subscribe(listener: () => void): () => void {
  listeners.add(listener);

  if (timer === null) {
    snapshot = Date.now();
    timer = setInterval(() => {
      snapshot = Date.now();
      listeners.forEach((notify) => notify());
    }, TICK_MS);
  }

  return () => {
    listeners.delete(listener);

    if (listeners.size === 0 && timer !== null) {
      clearInterval(timer);
      timer = null;
    }
  };
}

const getSnapshot = () => snapshot;

/** `0` marca "ainda não montou" - ver a explicação de prerender abaixo. */
const getServerSnapshot = () => 0;

/**
 * Rótulo de tempo decorrido ("há 3 dias") que se atualiza sozinho.
 *
 * Devolve a **data absoluta** no servidor e até a hidratação terminar. Isso não é detalhe: com
 * `cacheComponents`, ler o relógio durante o prerender congela o valor no build - a página
 * sairia dizendo "há 2 horas" para sempre. A data absoluta é correta no HTML inicial, é o que
 * os buscadores indexam, e o valor relativo entra depois sem divergência de hidratação.
 *
 * `useSyncExternalStore` em vez de `useState` + `useEffect` porque é exatamente isto: ler uma
 * fonte externa mutável (o relógio) com um snapshot de servidor declarado.
 */
export function useRelativeTime(isoDate: string | null | undefined): string {
  const now = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);

  return now === 0 ? formatDate(isoDate) : formatRelativeTime(isoDate, now);
}
