'use client';

import * as React from 'react';
import { RowActions, type RowAction } from './RowActions';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './Table';

export type DataTableDataColumn<TItem> = {
  key: string;
  header: React.ReactNode;
  render: (item: TItem) => React.ReactNode;
  align?: 'left' | 'center' | 'right';
};

export type DataTableActionsColumn<TItem> = {
  key: string;
  type: 'actions';
  /** Header opcional (por padrão a coluna de ações não tem título). */
  header?: React.ReactNode;
  getActions: (item: TItem) => RowAction[];
  maxInlineActions?: number;
};

export type DataTableColumn<TItem> = DataTableDataColumn<TItem> | DataTableActionsColumn<TItem>;

function isActionsColumn<TItem>(column: DataTableColumn<TItem>): column is DataTableActionsColumn<TItem> {
  return 'type' in column && column.type === 'actions';
}

export type DataTableProps<TItem> = {
  columns: ReadonlyArray<DataTableColumn<TItem>>;
  items: readonly TItem[];
  /** Key estável de cada linha (ex.: `(item) => item.id`). */
  getRowKey: (item: TItem) => React.Key;
};

/**
 * Tabela declarativa: o chamador define colunas (dados ou ações) e itens,
 * sem montar `TableHead`/`TableCell` manualmente.
 */
export function DataTable<TItem>({ columns, items, getRowKey }: DataTableProps<TItem>) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          {columns.map((column) =>
            isActionsColumn(column) ? (
              <TableHead key={column.key} aria-label="Ações">
                {column.header ?? null}
              </TableHead>
            ) : (
              <TableHead key={column.key} style={column.align ? { textAlign: column.align } : undefined}>
                {column.header}
              </TableHead>
            )
          )}
        </TableRow>
      </TableHeader>
      <TableBody>
        {items.map((item) => (
          <TableRow key={getRowKey(item)}>
            {columns.map((column) =>
              isActionsColumn(column) ? (
                <TableCell key={column.key}>
                  <RowActions actions={column.getActions(item)} maxInline={column.maxInlineActions} />
                </TableCell>
              ) : (
                <TableCell key={column.key} style={column.align ? { textAlign: column.align } : undefined}>
                  {column.render(item)}
                </TableCell>
              )
            )}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
