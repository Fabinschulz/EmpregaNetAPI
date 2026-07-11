'use client';

import { DEFAULT_PAGE, DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '@/shared/schema';
import { useCallback, useEffect, useState } from 'react';

const STORAGE_PREFIX = 'empreganet_table_pagination';

type PersistedState = { page: number; pageSize: number };

function buildStorageKey(stgKey: string): string {
  return `${STORAGE_PREFIX}:${stgKey}`;
}

function readPersistedState(stgKey: string): PersistedState | null {
  if (typeof window === 'undefined') return null;
  try {
    const raw = localStorage.getItem(buildStorageKey(stgKey));
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Partial<PersistedState>;
    if (typeof parsed.page !== 'number' || typeof parsed.pageSize !== 'number') return null;
    return { page: parsed.page, pageSize: parsed.pageSize };
  } catch {
    return null;
  }
}

function writePersistedState(stgKey: string, state: PersistedState) {
  if (typeof window === 'undefined') return;
  localStorage.setItem(buildStorageKey(stgKey), JSON.stringify(state));
}

function clearPersistedState(stgKey: string) {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(buildStorageKey(stgKey));
}

export type UseTablePaginationOptions = {
  /** Identificador único da tabela — chave de persistência (ex.: "recrutamento-vagas"). */
  storageKey: string;
  /** Página inicial (1-based) quando não há estado persistido. Padrão: 1. */
  defaultPage?: number;
  /** Tamanho de página inicial quando não há estado persistido. Padrão: `DEFAULT_PAGE_SIZE`. */
  defaultPageSize?: number;
  /** Opções exibidas no seletor de itens por página. Padrão: `PAGE_SIZE_OPTIONS`. */
  pageSizeOptions?: readonly number[];
  /** Desliga a persistência em localStorage, mantendo o estado só em memória. Padrão: `true`. */
  persist?: boolean;
};

export type UseTablePaginationResult = {
  page: number;
  pageSize: number;
  pageSizeOptions: readonly number[];
  /** Vai para a página informada (mínimo 1). */
  setPage: (page: number) => void;
  /** Troca os itens por página e reinicia para a primeira página. */
  setPageSize: (size: number) => void;
  /** Volta ao estado inicial e limpa a persistência. */
  reset: () => void;
};

/**
 * Estado de paginação de tabela (página + itens por página) persistido em `localStorage`
 * por `storageKey` — o usuário volta à tela na página em que parou.
 *
 * Não busca dados nem conhece totais: o estado alimenta a query (server-side: `{ page, size }`)
 * ou o fatiamento em memória (client-side), e o `<TablePagination totalItems={...}>` deriva
 * total de páginas, habilitação dos botões e o ajuste quando o total encolhe.
 */
export function usePersistedTablePagination(options: UseTablePaginationOptions): UseTablePaginationResult {
  const {
    storageKey,
    defaultPage = DEFAULT_PAGE,
    defaultPageSize = DEFAULT_PAGE_SIZE,
    pageSizeOptions = PAGE_SIZE_OPTIONS,
    persist = true
  } = options;

  const [page, setPageState] = useState<number>(() => {
    if (!persist) return defaultPage;
    return readPersistedState(storageKey)?.page ?? defaultPage;
  });
  const [pageSize, setPageSizeState] = useState<number>(() => {
    if (!persist) return defaultPageSize;
    return readPersistedState(storageKey)?.pageSize ?? defaultPageSize;
  });

  useEffect(() => {
    if (persist) writePersistedState(storageKey, { page, pageSize });
  }, [storageKey, persist, page, pageSize]);

  const setPage = useCallback((next: number) => {
    setPageState(Math.max(DEFAULT_PAGE, Math.trunc(next)));
  }, []);

  const setPageSize = useCallback((size: number) => {
    // Trocar o tamanho de página reinicia para a primeira página (comportamento padrão de tabelas).
    setPageSizeState(size);
    setPageState(DEFAULT_PAGE);
  }, []);

  const reset = useCallback(() => {
    clearPersistedState(storageKey);
    setPageState(defaultPage);
    setPageSizeState(defaultPageSize);
  }, [storageKey, defaultPage, defaultPageSize]);

  return { page, pageSize, pageSizeOptions, setPage, setPageSize, reset };
}
