'use client';

import { Skeleton } from '@/shared/components';
import styles from './dashboard-skeletons.module.scss';

export function KpiCardsSkeleton({ cards = 6 }: { cards?: number }) {
  return (
    <div className={styles.kpiGrid} role="status" aria-busy="true">
      <span className="sr-only">Carregando indicadores…</span>
      {Array.from({ length: cards }, (_, index) => (
        <div key={index} className={styles.kpiCard}>
          <Skeleton className={styles.kpiIcon} />
          <Skeleton className={styles.kpiLabel} />
          <Skeleton className={styles.kpiValue} />
          <Skeleton className={styles.kpiDelta} />
        </div>
      ))}
    </div>
  );
}

export function ChartSkeleton({ height = 260 }: { height?: number }) {
  const bars = [42, 68, 55, 80, 47, 73, 60, 88, 52, 66];

  return (
    <div className={styles.chart} style={{ height }} role="status" aria-busy="true">
      <span className="sr-only">Carregando gráfico…</span>
      <div className={styles.chartBars}>
        {bars.map((value, index) => (
          <Skeleton key={index} className={styles.chartBar} style={{ height: `${value}%` }} />
        ))}
      </div>
      <Skeleton className={styles.chartAxis} />
    </div>
  );
}

export function JobRowsSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className={styles.jobs} role="status" aria-busy="true">
      <span className="sr-only">Carregando vagas…</span>
      {Array.from({ length: rows }, (_, index) => (
        <div key={index} className={styles.jobRow}>
          <div className={styles.jobMain}>
            <Skeleton className={styles.jobTitle} />
            <Skeleton className={styles.jobMeta} />
          </div>
          <Skeleton className={styles.jobMetric} />
        </div>
      ))}
    </div>
  );
}

export function InsightsSkeleton({ rows = 3 }: { rows?: number }) {
  const widths = ['92%', '78%', '85%'];

  return (
    <div className={styles.insights} role="status" aria-busy="true">
      <span className="sr-only">Carregando análises…</span>
      {Array.from({ length: rows }, (_, index) => (
        <div key={index} className={styles.insightRow}>
          <Skeleton className={styles.insightIcon} />
          <Skeleton className={styles.insightText} style={{ width: widths[index % widths.length] }} />
        </div>
      ))}
    </div>
  );
}

export function FunnelSkeleton() {
  const stages = ['100%', '76%', '48%', '26%'];

  return (
    <div className={styles.funnel} role="status" aria-busy="true">
      <span className="sr-only">Carregando funil…</span>
      {stages.map((width, index) => (
        <div key={index} className={styles.funnelRow}>
          <Skeleton className={styles.funnelLabel} />
          <Skeleton className={styles.funnelBar} style={{ width }} />
        </div>
      ))}
    </div>
  );
}
