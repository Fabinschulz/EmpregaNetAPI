'use client';

import { notifyApiError, toastSuccess } from '@/shared/utils';
import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback, useState } from 'react';
import {
    fetchDashboardDistribution,
    fetchDashboardInsights,
    fetchDashboardJobs,
    fetchDashboardOverview,
    fetchDashboardTrends
} from './dashboard-api';
import { dashboardKeys } from './dashboard-keys';
import type { DashboardFilters, DashboardGranularity, DashboardJobRanking } from './dashboard-params';

const STALE_TIME_MS = 60_000;

export function useDashboardOverviewQuery(filters: DashboardFilters) {
  return useQuery({
    queryKey: dashboardKeys.overview(filters),
    queryFn: () => fetchDashboardOverview(filters),
    placeholderData: keepPreviousData,
    staleTime: STALE_TIME_MS
  });
}

export function useDashboardTrendsQuery(filters: DashboardFilters, granularity?: DashboardGranularity) {
  return useQuery({
    queryKey: dashboardKeys.trends(filters, granularity),
    queryFn: () => fetchDashboardTrends(filters, granularity),
    placeholderData: keepPreviousData,
    staleTime: STALE_TIME_MS
  });
}

export function useDashboardDistributionQuery(filters: DashboardFilters) {
  return useQuery({
    queryKey: dashboardKeys.distribution(filters),
    queryFn: () => fetchDashboardDistribution(filters),
    placeholderData: keepPreviousData,
    staleTime: STALE_TIME_MS
  });
}

export function useDashboardJobsQuery(
  filters: DashboardFilters,
  options: { ranking: DashboardJobRanking; limit?: number; onlyActive?: boolean }
) {
  const limit = options.limit ?? 8;
  const onlyActive = options.onlyActive ?? true;

  return useQuery({
    queryKey: dashboardKeys.jobs(filters, options.ranking, limit, onlyActive),
    queryFn: () => fetchDashboardJobs(filters, { ranking: options.ranking, limit, onlyActive }),
    placeholderData: keepPreviousData,
    staleTime: STALE_TIME_MS
  });
}

export function useDashboardInsightsQuery(filters: DashboardFilters) {
  return useQuery({
    queryKey: dashboardKeys.insights(filters),
    queryFn: () => fetchDashboardInsights(filters),
    placeholderData: keepPreviousData,
    staleTime: STALE_TIME_MS
  });
}

export function useDashboardRefresh() {
  const queryClient = useQueryClient();
  const [isRefreshing, setIsRefreshing] = useState(false);

  const refresh = useCallback(async () => {
    setIsRefreshing(true);
    try {
      await queryClient.invalidateQueries({ queryKey: dashboardKeys.all });
      toastSuccess('Métricas atualizadas', 'Os dados do dashboard foram recarregados.');
    } catch (error) {
      notifyApiError(error, 'atualizar as métricas', 'dashboard');
    } finally {
      setIsRefreshing(false);
    }
  }, [queryClient]);

  return { refresh, isRefreshing };
}
