'use client';

import type { UseTablePaginationResult } from '@/hooks';
import { cn } from '@/utils/lib';
import * as React from 'react';
import { ListRowsSkeleton } from '../../../common/loading/list-rows-skeleton';
import { Alert } from '../../molecules/alert';
import { DataTable, type DataTableColumn } from './DataTable';
import styles from './TableContainer.module.scss';
import { TablePagination } from './TablePagination';

export type TableContainerProps<TItem> = {
  /** Definição das colunas — de dados ou de ações (`type: 'actions'`, como no MUI). */
  columns: ReadonlyArray<DataTableColumn<TItem>>;
  /** Itens da página atual. */
  items: readonly TItem[];
  /** Key estável de cada linha (ex.: `(item) => item.id`). */
  getRowKey: (item: TItem) => React.Key;
  /** Painel de filtros (tipicamente um `<TableFilters>`), exibido acima da tabela. */
  filters?: React.ReactNode;
  /** Estado de paginação (de `usePersistedTablePagination`). Omitido = sem barra de paginação. */
  pagination?: UseTablePaginationResult;
  /** Total de itens da API — alimenta o intervalo/limites da paginação. */
  totalItems?: number;
  /** Primeira busca em andamento: exibe skeleton no lugar da tabela. */
  isPending?: boolean;
  /** Linhas do skeleton de carregamento. Padrão: 6. */
  skeletonRows?: number;
  /** Título do aviso exibido quando não há itens. Padrão: "Nenhum registro". */
  emptyTitle?: string;
  /** Mensagem exibida quando não há itens. */
  emptyMessage?: React.ReactNode;
  className?: string;
};

/**
 * Container completo de listagem em tabela: filtros em cima, tabela declarativa
 * no meio (com estados de carregamento e vazio já tratados) e paginação embaixo.
 * As telas informam apenas dados e definições — sem markup de tabela.
 */
export function TableContainer<TItem>({
  columns,
  items,
  getRowKey,
  filters,
  pagination,
  totalItems,
  isPending = false,
  skeletonRows = 6,
  emptyTitle = 'Nenhum registro',
  emptyMessage = 'Nenhum registro encontrado.',
  className
}: TableContainerProps<TItem>) {
  const hasItems = !isPending && items.length > 0;

  return (
    <div className={cn(styles.root, className)}>
      {filters}

      {isPending ? <ListRowsSkeleton rows={skeletonRows} /> : null}

      {!isPending && items.length === 0 ? <Alert title={emptyTitle}>{emptyMessage}</Alert> : null}

      {hasItems ? <DataTable columns={columns} items={items} getRowKey={getRowKey} /> : null}

      {hasItems && pagination ? <TablePagination pagination={pagination} totalItems={totalItems} /> : null}
    </div>
  );
}
