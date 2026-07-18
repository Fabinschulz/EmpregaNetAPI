'use client';

import { cn } from '@/utils';
import type { CSSProperties, ReactNode } from 'react';
import styles from './filter-bar.module.scss';

export type FilterBarProps = {
  /** Campos do filtro. Filhos diretos viram células do grid automaticamente — nenhum wrapper é necessário. */
  children: ReactNode;
  /** Ações ancoradas à direita (ex.: botão Limpar). */
  actions?: ReactNode;
  className?: string;
};

/**
 * Barra de filtros com grid automático: as colunas derivam do espaço disponível
 * (`auto-fill`), então cada campo colocado como filho direto já se encaixa sozinho,
 * com a mesma largura em todas as telas.
 * Para dar mais peso a um campo (ex.: busca), envolva-o em `<FilterField span={2}>`
 * ou `<FilterField span="full">` (linha inteira) — uso opcional, apenas para ênfase.
 */
export function FilterBar({ children, actions, className }: FilterBarProps) {
  return (
    <div className={cn(styles.bar, className)}>
      <div className={styles.fields}>{children}</div>
      {actions ? <div className={styles.actions}>{actions}</div> : null}
    </div>
  );
}

export type FilterFieldProps = {
  /** Peso do campo: nº de células que ocupa (ex.: 2) ou 'full' para a linha inteira. */
  span?: number | 'full';
  children: ReactNode;
  className?: string;
};

/**
 * Wrapper "OPCIONAL" de ênfase dentro da {@link FilterBar}: ocupa `span` células do
 * grid (ou a linha inteira com 'full'). Campos sem wrapper ocupam uma célula.
 */
export function FilterField({ span = 1, children, className }: FilterFieldProps) {
  const isFull = span === 'full';
  return (
    <div
      className={cn(styles.item, isFull && styles.itemFull, className)}
      style={!isFull ? ({ '--filter-span': span } as CSSProperties) : undefined}
    >
      {children}
    </div>
  );
}
