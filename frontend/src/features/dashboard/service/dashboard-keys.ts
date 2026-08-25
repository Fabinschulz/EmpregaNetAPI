import {
    dashboardFiltersToKey,
    type DashboardFilters,
    type DashboardGranularity,
    type DashboardJobRanking
} from './dashboard-params';

export const dashboardKeys = {
  all: ['dashboard'] as const,
  overview: (filters: DashboardFilters) => [...dashboardKeys.all, 'overview', dashboardFiltersToKey(filters)] as const,
  trends: (filters: DashboardFilters, granularity: DashboardGranularity | undefined) =>
    [...dashboardKeys.all, 'trends', dashboardFiltersToKey(filters), granularity ?? 'auto'] as const,
  distribution: (filters: DashboardFilters) =>
    [...dashboardKeys.all, 'distribution', dashboardFiltersToKey(filters)] as const,
  jobs: (filters: DashboardFilters, ranking: DashboardJobRanking, limit: number, onlyActive: boolean) =>
    [...dashboardKeys.all, 'jobs', dashboardFiltersToKey(filters), ranking, limit, onlyActive] as const,
  insights: (filters: DashboardFilters) => [...dashboardKeys.all, 'insights', dashboardFiltersToKey(filters)] as const
};
