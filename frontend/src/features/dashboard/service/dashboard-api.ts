import { axiosApi } from '@/shared/api';
import {
    dashboardFiltersToParams,
    type DashboardFilters,
    type DashboardGranularity,
    type DashboardJobRanking
} from './dashboard-params';
import {
    dashboardDistributionResponseSchema,
    dashboardInsightsResponseSchema,
    dashboardJobsResponseSchema,
    dashboardOverviewResponseSchema,
    dashboardTrendsResponseSchema,
    type DashboardDistributionResponse,
    type DashboardInsightsResponse,
    type DashboardJobsResponse,
    type DashboardOverviewResponse,
    type DashboardTrendsResponse
} from './dashboard-response-schema';

const BASE_URL = '/api/dashboard';

export async function fetchDashboardOverview(filters: DashboardFilters): Promise<DashboardOverviewResponse> {
  const res = await axiosApi.get(`${BASE_URL}/overview`, { params: dashboardFiltersToParams(filters) });
  return dashboardOverviewResponseSchema.parse(res.data);
}

export async function fetchDashboardTrends(
  filters: DashboardFilters,
  granularity?: DashboardGranularity
): Promise<DashboardTrendsResponse> {
  const res = await axiosApi.get(`${BASE_URL}/trends`, {
    params: { ...dashboardFiltersToParams(filters), granularity }
  });
  return dashboardTrendsResponseSchema.parse(res.data);
}

export async function fetchDashboardDistribution(filters: DashboardFilters): Promise<DashboardDistributionResponse> {
  const res = await axiosApi.get(`${BASE_URL}/distribution`, {
    params: dashboardFiltersToParams(filters)
  });
  return dashboardDistributionResponseSchema.parse(res.data);
}

export async function fetchDashboardJobs(
  filters: DashboardFilters,
  options: { ranking: DashboardJobRanking; limit?: number; onlyActive?: boolean }
): Promise<DashboardJobsResponse> {
  const res = await axiosApi.get(`${BASE_URL}/jobs`, {
    params: {
      ...dashboardFiltersToParams(filters),
      ranking: options.ranking,
      limit: options.limit,
      onlyActive: options.onlyActive
    }
  });
  return dashboardJobsResponseSchema.parse(res.data);
}

export async function fetchDashboardInsights(filters: DashboardFilters): Promise<DashboardInsightsResponse> {
  const res = await axiosApi.get(`${BASE_URL}/insights`, { params: dashboardFiltersToParams(filters) });
  return dashboardInsightsResponseSchema.parse(res.data);
}
