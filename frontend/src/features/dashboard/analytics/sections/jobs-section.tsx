'use client';

import { useQueryApiError } from '@/shared/hooks';
import { useState } from 'react';
import {
    DASHBOARD_JOB_RANKINGS,
    dashboardJobRankingLabels,
    useDashboardJobsQuery,
    type DashboardFilters,
    type DashboardJobRanking
} from '../../service';
import { JobPerformanceList } from '../jobs/job-performance-list';
import { DashboardPanel, resolvePanelState } from '../shared/dashboard-panel';
import { JobRowsSkeleton } from '../shared/dashboard-skeletons';
import { SegmentedControl } from '../shared/segmented-control';

const RANKING_OPTIONS = DASHBOARD_JOB_RANKINGS.map((ranking) => ({
  value: ranking,
  label: dashboardJobRankingLabels[ranking]
}));

export function JobsSection({ filters }: { filters: DashboardFilters }) {
  const [ranking, setRanking] = useState<DashboardJobRanking>('MostApplications');

  const query = useDashboardJobsQuery(filters, { ranking, limit: 8, onlyActive: true });
  const { message } = useQueryApiError(query.error, 'métricas');

  const items = query.data?.items ?? [];

  const listState = resolvePanelState({
    isPending: query.isPending,
    isFetching: query.isFetching,
    isError: query.isError,
    isEmpty: items.length === 0
  });

  const retry = () => query.refetch();

  return (
    <DashboardPanel
      title="Performance das vagas"
      description={
        query.data ? `${query.data.rankingLabel} · ${query.data.jobsInAverage} vagas na base de comparação` : undefined
      }
      hint="Candidaturas no período e acumuladas até o fim dele. A vaga não tem data de expiração: os indicadores de tempo são dias publicada e dias sem candidatura."
      state={listState}
      skeleton={<JobRowsSkeleton />}
      errorMessage={message}
      onRetry={retry}
      emptyMessage="Nenhuma vaga aberta encontrada para este recorte."
      actions={
        <SegmentedControl
          label="Critério do ranking de vagas"
          value={ranking}
          options={RANKING_OPTIONS}
          onChange={setRanking}
        />
      }
    >
      {query.data ? (
        <JobPerformanceList items={items} averageApplicationsPerJob={query.data.averageApplicationsPerJob} />
      ) : null}
    </DashboardPanel>
  );
}
