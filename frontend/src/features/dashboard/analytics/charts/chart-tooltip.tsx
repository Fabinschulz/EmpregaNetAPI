'use client';

import type { ReactNode } from 'react';
import { formatCount, formatPercent } from '../shared/dashboard-format';
import styles from './chart-tooltip.module.scss';

export type ChartTooltipRow = {
  key: string;
  label: string;
  value: number;
  color?: string;
  /** Percentagem exibida ao lado do valor, quando faz sentido para o gráfico. */
  share?: number | null;
};

export type ChartTooltipProps = {
  title?: ReactNode;
  rows: ChartTooltipRow[];
  footer?: ReactNode;
};

export function ChartTooltip({ title, rows, footer }: ChartTooltipProps) {
  if (rows.length === 0) return null;

  return (
    <div className={styles.tooltip} role="presentation">
      {title ? <p className={styles.title}>{title}</p> : null}
      <ul className={styles.rows}>
        {rows.map((row) => (
          <li key={row.key} className={styles.row}>
            <span className={styles.marker} style={{ background: row.color ?? 'var(--chart-neutral)' }} aria-hidden />
            <span className={styles.label}>{row.label}</span>
            <span className={styles.value}>
              {formatCount(row.value)}
              {typeof row.share === 'number' ? <span className={styles.share}>{formatPercent(row.share)}</span> : null}
            </span>
          </li>
        ))}
      </ul>
      {footer ? <p className={styles.footer}>{footer}</p> : null}
    </div>
  );
}
