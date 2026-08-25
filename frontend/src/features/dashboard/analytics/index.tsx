'use client';

import { FilterSection } from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useQueryApiError } from '@/shared/hooks';
import { useState } from 'react';
import {
    defaultDashboardFilters,
    useDashboardInsightsQuery,
    useDashboardOverviewQuery,
    useDashboardRefresh,
    type DashboardFilters
} from '../service';
import styles from './analytics.module.scss';
import {
    dashboardFilterFormSchema,
    defaultDashboardFilterForm,
    type DashboardFilterFormValues
} from './filters/dashboard-filter-schema';
import { DashboardFiltersBar } from './filters/dashboard-filters';
import { RecruitmentFunnel } from './funnel/recruitment-funnel';
import { DashboardHeader } from './header/dashboard-header';
import { InsightsPanel } from './insights/insights-panel';
import { KpiSection } from './kpis/kpi-section';
import { ApplicationStatusPanel, AreaDistributionPanel } from './sections/distribution-section';
import { JobsSection } from './sections/jobs-section';
import sectionStyles from './sections/sections.module.scss';
import { TrendsSection } from './sections/trends-section';
import { DashboardPanel, resolvePanelState } from './shared/dashboard-panel';
import { FunnelSkeleton } from './shared/dashboard-skeletons';

export function AnalyticsDashboard({ canSelectCompany }: { canSelectCompany: boolean }) {
  const [filters, setFilters] = useState<DashboardFilters>(defaultDashboardFilters);

  const overview = useDashboardOverviewQuery(filters);
  const insights = useDashboardInsightsQuery(filters);
  const { refresh, isRefreshing } = useDashboardRefresh();

  const overviewError = useQueryApiError(overview.error, 'métricas');
  const insightsError = useQueryApiError(insights.error, 'métricas');

  const funnelState = resolvePanelState({
    isPending: overview.isPending,
    isFetching: overview.isFetching,
    isError: overview.isError,
    isEmpty: (overview.data?.funnel.stages.length ?? 0) === 0
  });

  return (
    <div className={styles.page}>
      <DashboardHeader meta={overview.data?.meta} isRefreshing={isRefreshing} onRefresh={refresh} />

      <FilterSection
        title="Filtrar métricas"
        description="Refine os indicadores por período, localização, área e status da candidatura."
      >
        <FormProvider<DashboardFilterFormValues>
          validationSchema={dashboardFilterFormSchema}
          defaultValues={defaultDashboardFilterForm}
          onSubmit={() => undefined}
        >
          <DashboardFiltersBar onChange={setFilters} canSelectCompany={canSelectCompany} />
        </FormProvider>
      </FilterSection>

      <section aria-labelledby="dashboard-kpis" className={styles.section}>
        <KpiSection
          kpis={overview.data?.kpis ?? []}
          meta={overview.data?.meta}
          isPending={overview.isPending}
          isRefreshing={overview.isFetching && !overview.isPending}
          isError={overview.isError}
          errorMessage={overviewError.message}
          onRetry={() => overview.refetch()}
        />
      </section>

      <section aria-label="Evolução no período" className={styles.section}>
        <TrendsSection filters={filters} />
      </section>

      <section aria-label="Funil de conversão" className={styles.section}>
        <DashboardPanel
          title="Funil de recrutamento"
          hint="Começa na candidatura: a plataforma não registra visualizações de vaga. A etapa de aprovação soma aprovadas e concluídas, porque a candidatura guarda apenas o status atual."
          state={funnelState}
          skeleton={<FunnelSkeleton />}
          errorMessage={overviewError.message}
          onRetry={() => overview.refetch()}
        >
          {overview.data ? <RecruitmentFunnel funnel={overview.data.funnel} /> : null}
        </DashboardPanel>
      </section>

      <section aria-labelledby="dashboard-distribution" className={styles.section}>
        <div className={sectionStyles.gridAuto}>
          <ApplicationStatusPanel filters={filters} />
          <AreaDistributionPanel filters={filters} />
        </div>
      </section>

      <section aria-label="Performance das vagas" className={styles.section}>
        <JobsSection filters={filters} />
      </section>

      <section aria-labelledby="dashboard-insights" className={styles.section}>
        <InsightsPanel
          insights={insights.data?.items ?? []}
          isPending={insights.isPending}
          isRefreshing={insights.isFetching && !insights.isPending}
          isError={insights.isError}
          errorMessage={insightsError.message}
          onRetry={() => insights.refetch()}
        />
      </section>
    </div>
  );
}
