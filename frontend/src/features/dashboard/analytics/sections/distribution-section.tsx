'use client';

import { useQueryApiError } from '@/shared/hooks';
import { useState } from 'react';
import { useDashboardDistributionQuery, type DashboardBreakdown, type DashboardFilters } from '../../service';
import { BreakdownBarChart } from '../charts/breakdown-bar-chart';
import { StatusDonutChart } from '../charts/status-donut-chart';
import { DashboardPanel, resolvePanelState, type DashboardPanelState } from '../shared/dashboard-panel';
import { ChartSkeleton } from '../shared/dashboard-skeletons';
import { SegmentedControl } from '../shared/segmented-control';

function useDistributionPanels(filters: DashboardFilters) {
  const query = useDashboardDistributionQuery(filters);
  const { message } = useQueryApiError(query.error, 'métricas');

  const breakdownState = (breakdown: DashboardBreakdown | undefined): DashboardPanelState =>
    resolvePanelState({
      isPending: query.isPending,
      isFetching: query.isFetching,
      isError: query.isError,
      isEmpty: !breakdown || breakdown.items.length === 0
    });

  return { data: query.data, message, breakdownState, retry: () => query.refetch() };
}

const AREA_OPTIONS = [
  { value: 'applications' as const, label: 'Candidaturas' },
  { value: 'jobs' as const, label: 'Vagas' }
];

export function ApplicationStatusPanel({ filters }: { filters: DashboardFilters }) {
  const { data, message, breakdownState, retry } = useDistributionPanels(filters);

  return (
    <DashboardPanel
      title="Status das candidaturas"
      description="Composição do processo seletivo no período"
      hint="Mostra todos os status, mesmo com o filtro de status ativo."
      state={breakdownState(data?.applicationsByStatus)}
      skeleton={<ChartSkeleton height={220} />}
      errorMessage={message}
      onRetry={retry}
      emptyMessage="Nenhuma candidatura neste período."
      note={data?.applicationsByStatus.note}
    >
      {data ? <StatusDonutChart breakdown={data.applicationsByStatus} /> : null}
    </DashboardPanel>
  );
}

export function AreaDistributionPanel({ filters }: { filters: DashboardFilters }) {
  const [dimension, setDimension] = useState<'applications' | 'jobs'>('applications');
  const { data, message, breakdownState, retry } = useDistributionPanels(filters);

  const breakdown = dimension === 'applications' ? data?.applicationsByArea : data?.jobsByArea;

  return (
    <DashboardPanel
      title={dimension === 'applications' ? 'Candidaturas por área' : 'Vagas por área'}
      description={dimension === 'applications' ? 'Onde está a procura no período' : 'Onde está a oferta no período'}
      hint="A área do candidato não é cadastrada; a leitura por área vem sempre da vaga."
      state={breakdownState(breakdown)}
      skeleton={<ChartSkeleton />}
      errorMessage={message}
      onRetry={retry}
      emptyMessage="Nenhuma área com movimento neste período."
      note={breakdown?.note}
      actions={
        <SegmentedControl
          label="Dimensão da distribuição por área"
          value={dimension}
          options={AREA_OPTIONS}
          onChange={setDimension}
        />
      }
    >
      {breakdown ? (
        <BreakdownBarChart
          breakdown={breakdown}
          a11yCaption={dimension === 'applications' ? 'Candidaturas por área no período' : 'Vagas por área no período'}
        />
      ) : null}
    </DashboardPanel>
  );
}
