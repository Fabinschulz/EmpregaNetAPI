'use client';

import type { DashboardFunnel } from '../../service';
import { CHART_SERIES_COLORS } from '../charts/chart-theme';
import { formatCount, formatPercent } from '../shared/dashboard-format';
import styles from './recruitment-funnel.module.scss';

export function RecruitmentFunnel({ funnel }: { funnel: DashboardFunnel }) {
  const entryStage = funnel.stages.find((stage) => stage.key === 'applications');
  const entryValue = entryStage?.value ?? 0;

  return (
    <div className={styles.funnel}>
      <ol className={styles.stages}>
        {funnel.stages.map((stage, index) => {
          const isContext = stage.key === 'publishedJobs';
          const width = isContext || entryValue === 0 ? 100 : Math.max((stage.value / entryValue) * 100, 1.5);
          const shareOfEntry =
            stage.shareOfEntry != null && stage.shareOfEntry !== stage.shareOfPrevious ? stage.shareOfEntry : null;

          return (
            <li key={stage.key} className={styles.stage}>
              <div className={styles.stageHeader}>
                <span className={styles.stageLabel}>{stage.label}</span>
                <span className={styles.stageValue}>
                  {formatCount(stage.value)}
                  {stage.shareOfPrevious != null ? (
                    <span className={styles.stageShare}>{formatPercent(stage.shareOfPrevious)}</span>
                  ) : null}
                </span>
              </div>

              <div className={styles.stageTrack}>
                <span
                  className={styles.stageFill}
                  style={{
                    width: `${width}%`,
                    background: isContext
                      ? 'var(--chart-rest)'
                      : CHART_SERIES_COLORS[index % CHART_SERIES_COLORS.length]
                  }}
                />
              </div>

              {isContext ? (
                <p className={styles.stageCaption}>Contexto: as candidaturas abaixo não derivam só destas vagas.</p>
              ) : shareOfEntry != null ? (
                <p className={styles.stageCaption}>{formatPercent(shareOfEntry)} das candidaturas recebidas</p>
              ) : null}
            </li>
          );
        })}
      </ol>

      <p className={styles.conversion}>
        {funnel.conversionRate != null ? (
          <>
            <span className={styles.conversionValue}>{formatPercent(funnel.conversionRate)}</span>
            <span className={styles.conversionLabel}>das candidaturas chegaram à aprovação</span>
          </>
        ) : (
          <span className={styles.conversionLabel}>Sem candidaturas no período: não há conversão a calcular.</span>
        )}
      </p>
    </div>
  );
}
