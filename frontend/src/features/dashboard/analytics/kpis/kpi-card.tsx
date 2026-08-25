'use client';

import { actionIcons, Card, CardContent, Tooltip, TooltipContent, TooltipTrigger } from '@/shared/components';
import { cn } from '@/shared/utils';
import { ArrowDownRight, ArrowUpRight, Minus } from 'lucide-react';
import type { DashboardKpi } from '../../service';
import { formatKpiValue, formatSignedPercent } from '../shared/dashboard-format';
import { KPI_ICON_FALLBACK, KPI_ICONS } from './kpi-icons';
import styles from './kpi.module.scss';

export type KpiCardProps = {
  kpi: DashboardKpi;
  /** Intervalo do período anterior, em texto. Vai ao tooltip, não ao corpo do cartão. */
  previousPeriodLabel: string;
};

export function KpiCard({ kpi, previousPeriodLabel }: KpiCardProps) {
  const Icon = KPI_ICONS[kpi.key] ?? KPI_ICON_FALLBACK;
  const hasValue = kpi.value != null;

  const tone = kpi.trend === 'up' ? 'positive' : kpi.trend === 'down' ? 'negative' : 'neutral';
  const TrendIcon = kpi.trend === 'up' ? ArrowUpRight : kpi.trend === 'down' ? ArrowDownRight : Minus;

  const tooltip = [
    kpi.hint,
    kpi.isPeriodScoped ? null : 'Valor acumulado: é uma foto do fim do período, não o que aconteceu dentro dele.',
    kpi.changePercent != null ? `Comparado com ${previousPeriodLabel}.` : null
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <Card className={styles.card}>
      <CardContent className={styles.body}>
        <div className={styles.head}>
          <span className={styles.icon} aria-hidden>
            <Icon />
          </span>

          <Tooltip>
            <TooltipTrigger asChild>
              <button type="button" className={styles.hintButton} aria-label={`Como "${kpi.label}" é calculado`}>
                <actionIcons.info aria-hidden />
              </button>
            </TooltipTrigger>
            <TooltipContent className={styles.hintContent}>{tooltip}</TooltipContent>
          </Tooltip>
        </div>

        <p className={styles.label}>{kpi.label}</p>

        {kpi.value != null ? (
          <p className={styles.value}>{formatKpiValue(kpi.value, kpi.unit)}</p>
        ) : (
          <p className={styles.valueEmpty}>Sem registros</p>
        )}

        <div className={styles.delta}>
          {!hasValue ? (
            <span className={styles.deltaCaption}>Nenhum registro neste período</span>
          ) : kpi.changePercent != null ? (
            <>
              <span className={cn(styles.deltaChip, styles[tone])}>
                <TrendIcon className={styles.deltaIcon} aria-hidden />
                {formatSignedPercent(kpi.changePercent)}
              </span>
              <span className={styles.deltaCaption}>vs. período anterior</span>
            </>
          ) : null}
        </div>
      </CardContent>
    </Card>
  );
}
