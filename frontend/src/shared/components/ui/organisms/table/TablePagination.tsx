'use client';

import type { UseTablePaginationResult } from '@/hooks';
import { clampPage, computeTotalPages } from '@/shared/schema';
import { cn } from '@/utils/lib';
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import { useEffect } from 'react';
import { Button } from '../../atoms/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../molecules/select';
import styles from './TablePagination.module.scss';

export type TablePaginationProps = {
  /** Estado/ações vindos de `usePersistedTablePagination`. */
  pagination: UseTablePaginationResult;
  /**
   * Total de itens: `totalItems` da API (server-side) ou `items.length` (client-side).
   * `undefined` enquanto a primeira busca não terminou — a navegação avante fica desabilitada.
   */
  totalItems?: number;
  className?: string;
};

function rangeLabel(page: number, pageSize: number, totalItems: number | undefined): string {
  if (totalItems === undefined) return `Página ${page}`;
  if (totalItems === 0) return '0 de 0';
  const start = (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, totalItems);
  return `${start}–${end} de ${totalItems}`;
}


export function TablePagination({ pagination, totalItems, className }: TablePaginationProps) {
  const { page, pageSize, pageSizeOptions, setPage, setPageSize } = pagination;

  const totalPages = totalItems === undefined ? undefined : computeTotalPages(totalItems, pageSize);
  const canGoBack = page > 1;
  const canGoForward = totalPages !== undefined && page < totalPages;

  useEffect(() => {
    if (totalPages !== undefined && clampPage(page, totalPages) !== page) {
      setPage(clampPage(page, totalPages));
    }
  }, [page, totalPages, setPage]);

  return (
    <div className={cn(styles.root, className)} role="navigation" aria-label="Paginação da tabela">
      <div className={styles.pageSize}>
        <span className={styles.label}>Itens por página:</span>
        <Select value={String(pageSize)} onValueChange={(value) => setPageSize(Number(value))}>
          <SelectTrigger className={styles.pageSizeTrigger} aria-label="Itens por página">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {pageSizeOptions.map((option) => (
              <SelectItem key={option} value={String(option)}>
                {option}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <span className={styles.range} aria-live="polite">
        {rangeLabel(page, pageSize, totalItems)}
      </span>

      <div className={styles.actions}>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => setPage(1)}
          disabled={!canGoBack}
          aria-label="Primeira página"
        >
          <ChevronsLeft aria-hidden />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => setPage(page - 1)}
          disabled={!canGoBack}
          aria-label="Página anterior"
        >
          <ChevronLeft aria-hidden />
        </Button>
        <span className={styles.pageIndicator}>{totalPages === undefined ? page : `${page} / ${totalPages}`}</span>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => setPage(page + 1)}
          disabled={!canGoForward}
          aria-label="Próxima página"
        >
          <ChevronRight aria-hidden />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => {
            if (totalPages !== undefined) setPage(totalPages);
          }}
          disabled={!canGoForward}
          aria-label="Última página"
        >
          <ChevronsRight aria-hidden />
        </Button>
      </div>
    </div>
  );
}
