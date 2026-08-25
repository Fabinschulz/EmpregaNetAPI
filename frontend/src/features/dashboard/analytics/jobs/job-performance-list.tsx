'use client';

import { jobsRoutes } from '@/features/recrutamento/vagas/jobs-routes';
import { StatusBadge } from '@/shared/components';
import { cn } from '@/shared/utils';
import { ArrowDownRight, ArrowUpRight, ChevronRight, Minus } from 'lucide-react';
import Link from 'next/link';
import type { DashboardJobPerformance } from '../../service';
import { formatCount, formatDays, formatDecimal, formatSignedPercent } from '../shared/dashboard-format';
import styles from './job-performance.module.scss';

export type JobPerformanceListProps = {
  items: DashboardJobPerformance[];
  averageApplicationsPerJob: number;
};

export function JobPerformanceList({ items, averageApplicationsPerJob }: JobPerformanceListProps) {
  return (
    <>
      <p className={styles.baseline}>
        Média de candidaturas por vaga no recorte: <strong>{formatDecimal(averageApplicationsPerJob)}</strong>
      </p>

      <ul className={styles.list}>
        {items.map((job) => (
          <li key={job.id} className={styles.item}>
            <Link href={jobsRoutes.detail(job.id)} className={styles.link}>
              <div className={styles.main}>
                <div className={styles.titleRow}>
                  <span className={styles.title}>{job.title}</span>
                  <StatusBadge label={job.statusLabel} tone={job.isActive ? 'positive' : 'negative'} />
                </div>

                <p className={styles.meta}>
                  <span>{job.companyName}</span>
                  <span aria-hidden>·</span>
                  <span>
                    {job.city}/{job.state}
                  </span>
                  {job.areaLabel ? (
                    <>
                      <span aria-hidden>·</span>
                      <span>{job.areaLabel}</span>
                    </>
                  ) : null}
                </p>

                <p className={styles.timeline}>
                  <span>Publicada em {job.publishedAt}</span>
                  <span aria-hidden>·</span>
                  <span>{formatDays(job.daysActive)} ativa</span>
                  <span aria-hidden>·</span>
                  <span>
                    {job.daysSinceLastApplication == null
                      ? 'Nenhuma candidatura recebida'
                      : `Última candidatura há ${formatDays(job.daysSinceLastApplication)}`}
                  </span>
                </p>
              </div>

              <div className={styles.metrics}>
                <span className={styles.metricValue}>{formatCount(job.applications)}</span>
                <span className={styles.metricLabel}>no período</span>

                <span className={styles.metricTotal}>{formatCount(job.totalApplications)} no total</span>

                {job.performanceVsAverage != null ? (
                  <span className={cn(styles.performance, styles[job.performance])}>
                    <PerformanceIcon performance={job.performance} />
                    {formatSignedPercent(job.performanceVsAverage)} vs. média
                  </span>
                ) : (
                  <span className={cn(styles.performance, styles.none)}>Sem base de comparação</span>
                )}
              </div>

              <ChevronRight className={styles.chevron} aria-hidden />
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
}

function PerformanceIcon({ performance }: { performance: DashboardJobPerformance['performance'] }) {
  if (performance === 'above') return <ArrowUpRight className={styles.performanceIcon} aria-hidden />;
  if (performance === 'below') return <ArrowDownRight className={styles.performanceIcon} aria-hidden />;
  return <Minus className={styles.performanceIcon} aria-hidden />;
}
