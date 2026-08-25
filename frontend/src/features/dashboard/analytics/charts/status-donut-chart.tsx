'use client';

import { useMemo } from 'react';
import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts';
import type { DashboardBreakdown } from '../../service';
import { formatCount, formatPercent } from '../shared/dashboard-format';
import { ChartA11yTable } from './chart-a11y';
import { CHART_HEIGHT_COMPACT, statusColor } from './chart-theme';
import { ChartTooltip } from './chart-tooltip';
import styles from './charts.module.scss';
import type { RechartsTooltipProps } from './recharts-types';

export function StatusDonutChart({ breakdown }: { breakdown: DashboardBreakdown }) {
  const rows = useMemo(
    () => breakdown.items.map((item) => ({ ...item, color: statusColor(item.key) })),
    [breakdown.items]
  );

  const renderTooltip = ({ active, payload }: RechartsTooltipProps) => {
    if (!active || !payload?.length) return null;

    const row = payload[0]?.payload as (typeof rows)[number] | undefined;
    if (!row) return null;

    return (
      <ChartTooltip rows={[{ key: row.key, label: row.label, value: row.value, share: row.share, color: row.color }]} />
    );
  };

  return (
    <div className={styles.donutLayout}>
      <div className={styles.donutChart} style={{ height: CHART_HEIGHT_COMPACT }}>
        <ResponsiveContainer width="100%" height="100%">
          <PieChart accessibilityLayer>
            <Tooltip content={renderTooltip} />
            <Pie
              data={rows}
              dataKey="value"
              nameKey="label"
              innerRadius="62%"
              outerRadius="88%"
              paddingAngle={2}
              strokeWidth={0}
              isAnimationActive={false}
            >
              {rows.map((row) => (
                <Cell key={row.key} fill={row.color} />
              ))}
            </Pie>
          </PieChart>
        </ResponsiveContainer>

        <div className={styles.donutCenter} aria-hidden>
          <span className={styles.donutTotal}>{formatCount(breakdown.categorized)}</span>
          <span className={styles.donutCaption}>candidaturas</span>
        </div>
      </div>

      <ul className={styles.donutLegend}>
        {rows.map((row) => (
          <li key={row.key} className={styles.donutLegendItem}>
            <span className={styles.donutLegendMarker} style={{ background: row.color }} aria-hidden />
            <span className={styles.donutLegendLabel}>{row.label}</span>
            <span className={styles.donutLegendValue}>
              {formatCount(row.value)}
              <span className={styles.donutLegendShare}>{formatPercent(row.share)}</span>
            </span>
          </li>
        ))}
      </ul>

      <ChartA11yTable caption="Candidaturas por status no período" rows={rows} />
    </div>
  );
}
