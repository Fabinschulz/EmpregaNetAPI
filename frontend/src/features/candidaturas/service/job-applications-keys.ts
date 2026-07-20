import type { ListQueryParams } from '@/shared/schema';

export const jobApplicationsKeys = {
  all: ['job-applications'] as const,
  mine: (params: ListQueryParams) => [...jobApplicationsKeys.all, 'mine', params] as const,
  allList: (params: ListQueryParams) => [...jobApplicationsKeys.all, 'all', params] as const,
  byJobLists: () => [...jobApplicationsKeys.all, 'by-job'] as const,
  byJob: (jobId: number, params: ListQueryParams) => [...jobApplicationsKeys.byJobLists(), jobId, params] as const
};
