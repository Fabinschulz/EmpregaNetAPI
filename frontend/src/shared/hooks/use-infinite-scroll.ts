'use client';

import { useEffect, useRef } from 'react';

type UseInfiniteScrollOptions = {
  hasMore: boolean;
  isLoading: boolean;
  onLoadMore: () => void;
  rootMargin?: string;
};

/**
 * Dispara `onLoadMore` quando a sentinela devolvida entra (ou se aproxima) do viewport.
 *
 * `IntersectionObserver` em vez de escutar `scroll`: o callback roda fora do caminho crítico de
 * rolagem, então a lista não engasga enquanto o utilizador desce.
 *
 * @returns ref a colocar num elemento logo abaixo do último item da lista.
 */
export function useInfiniteScroll({
  hasMore,
  isLoading,
  onLoadMore,
  rootMargin = '0px 0px 600px 0px'
}: UseInfiniteScrollOptions) {
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  // O callback muda a cada render (fecha sobre estado da query); guardá-lo numa ref evita
  // recriar o observer e reprocessar a interseção a cada render.
  const onLoadMoreRef = useRef(onLoadMore);
  useEffect(() => {
    onLoadMoreRef.current = onLoadMore;
  }, [onLoadMore]);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel || !hasMore || isLoading) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          onLoadMoreRef.current();
        }
      },
      { rootMargin }
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasMore, isLoading, rootMargin]);

  return sentinelRef;
}
