import { z } from 'zod';

const dashboardScopeSchema = z.object({
  level: z.enum(['platform', 'company']),
  companyId: z.number().int().nullable().optional(),
  companyName: z.string().nullable().optional()
});

const dashboardUnavailableMetricSchema = z.object({
  metric: z.string(),
  label: z.string(),
  reason: z.string()
});

export const dashboardMetaSchema = z.object({
  period: z.string(),
  periodLabel: z.string(),
  from: z.string(),
  to: z.string(),
  fromUtc: z.string(),
  toUtcExclusive: z.string(),
  days: z.number().int().nonnegative(),
  previousFrom: z.string(),
  previousTo: z.string(),
  timezone: z.string(),
  generatedAt: z.string(),
  scope: dashboardScopeSchema,
  appliedFilters: z.array(z.string()).default([]),
  unavailable: z.array(dashboardUnavailableMetricSchema).default([])
});

export type DashboardMeta = z.infer<typeof dashboardMetaSchema>;
export type DashboardUnavailableMetric = z.infer<typeof dashboardUnavailableMetricSchema>;

const dashboardTrendSchema = z.enum(['up', 'down', 'flat', 'none']);
export type DashboardTrendDirection = z.infer<typeof dashboardTrendSchema>;

export const dashboardKpiSchema = z.object({
  key: z.string(),
  label: z.string(),
  value: z.number().nullable(),
  unit: z.string().nullable().optional(),
  previousValue: z.number().nullable().optional(),
  changePercent: z.number().nullable().optional(),
  trend: dashboardTrendSchema,
  hint: z.string(),
  isPeriodScoped: z.boolean()
});

export type DashboardKpi = z.infer<typeof dashboardKpiSchema>;

const dashboardFunnelStageSchema = z.object({
  key: z.string(),
  label: z.string(),
  value: z.number().int().nonnegative(),
  shareOfPrevious: z.number().nullable().optional(),
  shareOfEntry: z.number().nullable().optional()
});

export type DashboardFunnelStage = z.infer<typeof dashboardFunnelStageSchema>;

const dashboardFunnelSchema = z.object({
  stages: z.array(dashboardFunnelStageSchema),
  conversionRate: z.number().nullable().optional(),
  note: z.string()
});

export type DashboardFunnel = z.infer<typeof dashboardFunnelSchema>;

export const dashboardOverviewResponseSchema = z.object({
  meta: dashboardMetaSchema,
  kpis: z.array(dashboardKpiSchema),
  funnel: dashboardFunnelSchema
});

export type DashboardOverviewResponse = z.infer<typeof dashboardOverviewResponseSchema>;

const dashboardSeriesPointSchema = z.object({
  date: z.string(),
  label: z.string(),
  value: z.number().int()
});

export type DashboardSeriesPoint = z.infer<typeof dashboardSeriesPointSchema>;

const dashboardSeriesSchema = z.object({
  key: z.string(),
  label: z.string(),
  total: z.number().int().nonnegative(),
  points: z.array(dashboardSeriesPointSchema)
});

export type DashboardSeries = z.infer<typeof dashboardSeriesSchema>;

export const dashboardTrendsResponseSchema = z.object({
  meta: dashboardMetaSchema,
  granularity: z.string(),
  granularityLabel: z.string(),
  series: z.array(dashboardSeriesSchema)
});

export type DashboardTrendsResponse = z.infer<typeof dashboardTrendsResponseSchema>;

const dashboardBreakdownItemSchema = z.object({
  key: z.string(),
  label: z.string(),
  value: z.number().int().nonnegative(),
  share: z.number()
});

export type DashboardBreakdownItem = z.infer<typeof dashboardBreakdownItemSchema>;

const dashboardBreakdownSchema = z.object({
  items: z.array(dashboardBreakdownItemSchema),
  total: z.number().int().nonnegative(),
  categorized: z.number().int().nonnegative(),
  note: z.string().nullable().optional()
});

export type DashboardBreakdown = z.infer<typeof dashboardBreakdownSchema>;

export const dashboardDistributionResponseSchema = z.object({
  meta: dashboardMetaSchema,
  applicationsByStatus: dashboardBreakdownSchema,
  applicationsByArea: dashboardBreakdownSchema,
  jobsByArea: dashboardBreakdownSchema
});

export type DashboardDistributionResponse = z.infer<typeof dashboardDistributionResponseSchema>;

const dashboardJobPerformanceSchema = z.object({
  id: z.number().int(),
  title: z.string(),
  companyId: z.number().int(),
  companyName: z.string(),
  city: z.string(),
  state: z.string(),
  area: z.string(),
  areaLabel: z.string(),
  isActive: z.boolean(),
  statusLabel: z.string(),
  publishedAt: z.string(),
  daysActive: z.number().int().nonnegative(),
  applications: z.number().int().nonnegative(),
  totalApplications: z.number().int().nonnegative(),
  lastApplicationAt: z.string().nullable().optional(),
  daysSinceLastApplication: z.number().int().nullable().optional(),
  performanceVsAverage: z.number().nullable().optional(),
  performance: z.enum(['above', 'average', 'below', 'none'])
});

export type DashboardJobPerformance = z.infer<typeof dashboardJobPerformanceSchema>;

export const dashboardJobsResponseSchema = z.object({
  meta: dashboardMetaSchema,
  ranking: z.string(),
  rankingLabel: z.string(),
  onlyActive: z.boolean(),
  averageApplicationsPerJob: z.number(),
  jobsInAverage: z.number().int().nonnegative(),
  items: z.array(dashboardJobPerformanceSchema)
});

export type DashboardJobsResponse = z.infer<typeof dashboardJobsResponseSchema>;

export const DASHBOARD_INSIGHT_CATEGORIES = ['attention', 'growth', 'highlight', 'behavior'] as const;

const dashboardInsightCategorySchema = z.enum(DASHBOARD_INSIGHT_CATEGORIES);

export type DashboardInsightCategory = z.infer<typeof dashboardInsightCategorySchema>;

const dashboardInsightSchema = z.object({
  code: z.string(),
  category: dashboardInsightCategorySchema,
  tone: z.enum(['positive', 'negative', 'warning', 'neutral']),
  severity: z.enum(['low', 'medium', 'high']),
  title: z.string(),
  message: z.string()
});

export type DashboardInsight = z.infer<typeof dashboardInsightSchema>;

export const dashboardInsightsResponseSchema = z.object({
  meta: dashboardMetaSchema,
  items: z.array(dashboardInsightSchema),
  staleDays: z.number().int().positive()
});

export type DashboardInsightsResponse = z.infer<typeof dashboardInsightsResponseSchema>;
