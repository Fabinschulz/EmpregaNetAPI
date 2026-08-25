'use client';

import { actionIcons, Button } from '@/shared/components';
import type { DashboardKpi, DashboardMeta } from '../../service';
import { KpiCardsSkeleton } from '../shared/dashboard-skeletons';
import sectionStyles from '../shared/section-feedback.module.scss';
import { KpiCard } from './kpi-card';
import styles from './kpi.module.scss';

export type KpiSectionProps = {
  kpis: DashboardKpi[];
  meta: DashboardMeta | undefined;
  isPending: boolean;
  isRefreshing: boolean;
  isError: boolean;
  errorMessage?: string | null;
  onRetry: () => void;
};

export function KpiSection({ kpis, meta, isPending, isRefreshing, isError, errorMessage, onRetry }: KpiSectionProps) {
  if (isPending) {
    return <KpiCardsSkeleton />;
  }

  if (isError) {
    return (
      <div className={sectionStyles.feedback} role="alert">
        <p className={sectionStyles.title}>Não foi possível carregar os indicadores.</p>
        {errorMessage ? <p className={sectionStyles.text}>{errorMessage}</p> : null}
        <Button type="button" variant="outline" size="sm" startIcon={actionIcons.retry} onClick={onRetry}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  if (kpis.length === 0) {
    return (
      <div className={sectionStyles.feedback} role="status">
        <p className={sectionStyles.title}>Ainda não existem dados suficientes para este período.</p>
      </div>
    );
  }

  const previousPeriodLabel = meta ? `${meta.previousFrom} – ${meta.previousTo}` : 'o período anterior';

  return (
    <div className={styles.grid} data-refreshing={isRefreshing || undefined} aria-busy={isRefreshing}>
      {kpis.map((kpi) => (
        <KpiCard key={kpi.key} kpi={kpi} previousPeriodLabel={previousPeriodLabel} />
      ))}
    </div>
  );
}
