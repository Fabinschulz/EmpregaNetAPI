'use client';

import { useQueryApiError } from '@/shared/hooks';
import { cn } from '@/shared/utils';
import { useMemo, useState } from 'react';
import {
    DASHBOARD_GRANULARITIES,
    dashboardGranularityLabels,
    useDashboardTrendsQuery,
    type DashboardFilters,
    type DashboardGranularity
} from '../../service';
import { CHART_HEIGHT_HERO, seriesColor } from '../charts/chart-theme';
import { TrendAreaChart } from '../charts/trend-area-chart';
import { formatCount } from '../shared/dashboard-format';
import { DashboardPanel, resolvePanelState } from '../shared/dashboard-panel';
import { ChartSkeleton } from '../shared/dashboard-skeletons';
import { SegmentedControl } from '../shared/segmented-control';
import styles from './sections.module.scss';

const GRANULARITY_OPTIONS = DASHBOARD_GRANULARITIES.map((granularity) => ({
  value: granularity,
  label: dashboardGranularityLabels[granularity]
}));

export function TrendsSection({ filters }: { filters: DashboardFilters }) {
  const [granularity, setGranularity] = useState<DashboardGranularity | undefined>(undefined);
  const [hiddenKeys, setHiddenKeys] = useState<string[]>([]);

  const query = useDashboardTrendsQuery(filters, granularity);
  const { message } = useQueryApiError(query.error, 'métricas');

  const series = useMemo(() => query.data?.series ?? [], [query.data]);

  const visibleKeys = useMemo(
    () => series.filter((item) => !hiddenKeys.includes(item.key)).map((item) => item.key),
    [series, hiddenKeys]
  );

  const hasAnyValue = series.some((item) => item.total > 0);

  const state = resolvePanelState({
    isPending: query.isPending,
    isFetching: query.isFetching,
    isError: query.isError,
    isEmpty: series.length === 0 || !hasAnyValue
  });

  const toggleSeries = (key: string) => {
    setHiddenKeys((current) => {
      const willHide = !current.includes(key);
      
      if (willHide && visibleKeys.length === 1) return current;
      return willHide ? [...current, key] : current.filter((item) => item !== key);
    });
  };

  return (
    <DashboardPanel
      title="Evolução no período"
      description={query.data ? query.data.granularityLabel : undefined}
      hint="Cada ponto soma o que aconteceu no balde. Baldes sem movimento aparecem com zero - omiti-los desenharia uma linha reta sobre a lacuna."
      state={state}
      skeleton={<ChartSkeleton height={CHART_HEIGHT_HERO} />}
      errorMessage={message}
      onRetry={() => query.refetch()}
      emptyMessage="Nenhuma movimentação encontrada para o período selecionado."
      actions={
        <SegmentedControl
          label="Granularidade da série"
          value={granularity ?? (query.data?.granularity as DashboardGranularity | undefined) ?? 'Daily'}
          options={GRANULARITY_OPTIONS}
          onChange={setGranularity}
        />
      }
    >
      <ul className={styles.legend}>
        {series.map((item, index) => {
          const isHidden = hiddenKeys.includes(item.key);

          return (
            <li key={item.key}>
              <button
                type="button"
                className={cn(styles.legendButton, isHidden && styles.legendButtonOff)}
                aria-pressed={!isHidden}
                onClick={() => toggleSeries(item.key)}
              >
                <span
                  className={styles.legendMarker}
                  style={{ background: isHidden ? 'var(--chart-neutral)' : seriesColor(index) }}
                  aria-hidden
                />
                <span className={styles.legendLabel}>{item.label}</span>
                <span className={styles.legendTotal}>{formatCount(item.total)}</span>
              </button>
            </li>
          );
        })}
      </ul>

      <TrendAreaChart series={series} visibleKeys={visibleKeys} height={CHART_HEIGHT_HERO} />
    </DashboardPanel>
  );
}
