'use client';

import { useId, useMemo } from 'react';
import { Area, AreaChart, Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardSeries } from '../../service';
import { formatCompact, formatCount } from '../shared/dashboard-format';
import { ChartA11yTable } from './chart-a11y';
import {
    CHART_AXIS_PROPS,
    CHART_GRID_PROPS,
    CHART_HEIGHT,
    CHART_HEIGHT_SPARSE,
    SPARSE_SERIES_THRESHOLD,
    seriesColor
} from './chart-theme';
import { ChartTooltip } from './chart-tooltip';
import styles from './charts.module.scss';
import type { RechartsTooltipProps } from './recharts-types';


export type TrendAreaChartProps = {
  series: DashboardSeries[];
  /** Chaves visíveis; permite ao utilizador isolar uma série sem refazer a consulta. */
  visibleKeys: string[];
  height?: number;
};

type TrendRow = { date: string; label: string } & Record<string, number | string>;

export function TrendAreaChart({ series, visibleKeys, height = CHART_HEIGHT }: TrendAreaChartProps) {
  const gradientId = useId();
  const visible = useMemo(() => series.filter((item) => visibleKeys.includes(item.key)), [series, visibleKeys]);

  const rows = useMemo<TrendRow[]>(() => {
    const base = visible[0]?.points ?? [];

    return base.map((point, index) => {
      const row: TrendRow = { date: point.date, label: point.label };

      for (const item of visible) {
        row[item.key] = item.points[index]?.value ?? 0;
      }

      return row;
    });
  }, [visible]);

  const colorByKey = useMemo(() => {
    const map = new Map<string, string>();
    series.forEach((item, index) => map.set(item.key, seriesColor(index)));
    return map;
  }, [series]);

  const labelByKey = useMemo(() => {
    const map = new Map<string, string>();
    series.forEach((item) => map.set(item.key, item.label));
    return map;
  }, [series]);

  const renderTooltip = ({ active, payload, label }: RechartsTooltipProps) => {
    if (!active || !payload?.length) return null;

    return (
      <ChartTooltip
        title={label}
        rows={payload.map((entry) => {
          const key = String(entry.dataKey ?? entry.name ?? '');
          return {
            key,
            label: labelByKey.get(key) ?? key,
            value: Number(entry.value ?? 0),
            color: colorByKey.get(key)
          };
        })}
      />
    );
  };

  const isSparse = rows.length < SPARSE_SERIES_THRESHOLD;
  const resolvedHeight = isSparse ? CHART_HEIGHT_SPARSE : height;

  if (isSparse) {
    return (
      <>
        <div className={styles.chartArea} style={{ height: resolvedHeight }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={rows} margin={{ top: 4, right: 8, left: 0, bottom: 0 }} accessibilityLayer>
              <CartesianGrid {...CHART_GRID_PROPS} />
              <XAxis dataKey="label" {...CHART_AXIS_PROPS} />
              <YAxis {...CHART_AXIS_PROPS} width={44} allowDecimals={false} tickFormatter={formatCompact} />
              <Tooltip content={renderTooltip} cursor={{ fill: 'var(--chart-track)' }} />

              {visible.map((item) => (
                <Bar
                  key={item.key}
                  dataKey={item.key}
                  name={item.label}
                  fill={colorByKey.get(item.key)}
                  radius={[3, 3, 0, 0]}
                  maxBarSize={48}
                />
              ))}
            </BarChart>
          </ResponsiveContainer>
        </div>

        <TrendA11ySummary visible={visible} />
      </>
    );
  }

  return (
    <>
      <div className={styles.chartArea} style={{ height: resolvedHeight }}>
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={rows} margin={{ top: 4, right: 8, left: 0, bottom: 0 }} accessibilityLayer>
            <defs>
              {visible.map((item) => (
                <linearGradient key={item.key} id={`${gradientId}-${item.key}`} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor={colorByKey.get(item.key)} stopOpacity={0.28} />
                  <stop offset="100%" stopColor={colorByKey.get(item.key)} stopOpacity={0.02} />
                </linearGradient>
              ))}
            </defs>

            <CartesianGrid {...CHART_GRID_PROPS} />
            <XAxis dataKey="label" {...CHART_AXIS_PROPS} minTickGap={16} />
            <YAxis {...CHART_AXIS_PROPS} width={44} allowDecimals={false} tickFormatter={formatCompact} />
            <Tooltip content={renderTooltip} cursor={{ stroke: 'var(--chart-grid)', strokeWidth: 1 }} />

            {visible.map((item) => (
              <Area
                key={item.key}
                type="monotone"
                dataKey={item.key}
                name={item.label}
                stroke={colorByKey.get(item.key)}
                strokeWidth={2}
                fill={`url(#${gradientId}-${item.key})`}
                dot={false}
                activeDot={{ r: 4, strokeWidth: 2 }}
              />
            ))}
          </AreaChart>
        </ResponsiveContainer>
      </div>

      <TrendA11ySummary visible={visible} />
    </>
  );
}

function TrendA11ySummary({ visible }: { visible: DashboardSeries[] }) {
  return (
    <>
      <ChartA11yTable
        caption={`Evolução no período: ${visible.map((item) => item.label).join(', ')}`}
        rows={visible.map((item) => ({
          key: item.key,
          label: `${item.label} (total)`,
          value: item.total
        }))}
      />
      <span className="sr-only">
        {visible.map((item) => `${item.label}: ${formatCount(item.total)} no período.`).join(' ')}
      </span>
    </>
  );
}
