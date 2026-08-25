'use client';

import { useMemo } from 'react';
import { Bar, BarChart, Cell, LabelList, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardBreakdown } from '../../service';
import { formatCount } from '../shared/dashboard-format';
import { ChartA11yTable } from './chart-a11y';
import { CHART_AXIS_PROPS, seriesColor, statusColor } from './chart-theme';
import { ChartTooltip } from './chart-tooltip';
import styles from './charts.module.scss';
import type { RechartsTooltipProps } from './recharts-types';

export type BreakdownBarChartProps = {
  breakdown: DashboardBreakdown;
  /** `status` usa a paleta semântica; `category` usa a sequência categórica. */
  palette?: 'category' | 'status';
  a11yCaption: string;
};

const BAR_HEIGHT = 38;
const CHART_MIN_HEIGHT = 92;
/** Folga à direita para o número na ponta da barra mais longa não sair do desenho. */
const VALUE_LABEL_SPACE = 48;

export function BreakdownBarChart({ breakdown, palette = 'category', a11yCaption }: BreakdownBarChartProps) {
  const rows = useMemo(
    () =>
      breakdown.items.map((item, index) => ({
        ...item,
        color: palette === 'status' ? statusColor(item.key) : seriesColor(index, item.key)
      })),
    [breakdown.items, palette]
  );

  const renderTooltip = ({ active, payload }: RechartsTooltipProps) => {
    if (!active || !payload?.length) return null;

    const row = payload[0]?.payload as (typeof rows)[number] | undefined;
    if (!row) return null;

    return (
      <ChartTooltip rows={[{ key: row.key, label: row.label, value: row.value, share: row.share, color: row.color }]} />
    );
  };

  const height = Math.max(rows.length * BAR_HEIGHT, CHART_MIN_HEIGHT);

  return (
    <>
      <div className={styles.chartArea} style={{ height }}>
        <ResponsiveContainer width="100%" height="100%">
          <BarChart
            data={rows}
            layout="vertical"
            margin={{ top: 4, right: VALUE_LABEL_SPACE, left: 0, bottom: 4 }}
            accessibilityLayer
          >
            <XAxis type="number" hide />
            <YAxis
              type="category"
              dataKey="label"
              {...CHART_AXIS_PROPS}
              width={132}
              tickFormatter={(value: string) => (value.length > 28 ? `${value.slice(0, 27)}…` : value)}
            />
            <Tooltip content={renderTooltip} cursor={{ fill: 'var(--chart-track)' }} />
            <Bar dataKey="value" radius={[0, 4, 4, 0]} maxBarSize={22} isAnimationActive={false}>
              {rows.map((row) => (
                <Cell key={row.key} fill={row.color} />
              ))}
              <LabelList
                dataKey="value"
                position="right"
                offset={10}
                fill="var(--text)"
                fontSize={12}
                fontWeight={600}
                formatter={(value: unknown) => formatCount(Number(value))}
              />
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      <ChartA11yTable caption={a11yCaption} rows={rows} />
    </>
  );
}
