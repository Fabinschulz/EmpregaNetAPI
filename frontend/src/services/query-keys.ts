import type { ListQueryParams } from './shared';

export const queryKeys = {
  jobs: {
    all: ['jobs'] as const,
    lists: () => [...queryKeys.jobs.all, 'list'] as const,
    list: (params: ListQueryParams) => [...queryKeys.jobs.lists(), params] as const,
    details: () => [...queryKeys.jobs.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.jobs.details(), id] as const
  },
  companies: {
    all: ['companies'] as const,
    lists: () => [...queryKeys.companies.all, 'list'] as const,
    list: (params: ListQueryParams) => [...queryKeys.companies.lists(), params] as const,
    details: () => [...queryKeys.companies.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.companies.details(), id] as const
  },
  adminUsers: {
    all: ['admin-users'] as const,
    lists: () => [...queryKeys.adminUsers.all, 'list'] as const,
    list: (params: ListQueryParams) => [...queryKeys.adminUsers.lists(), params] as const,
    details: () => [...queryKeys.adminUsers.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.adminUsers.details(), id] as const
  },
  candidates: {
    all: ['candidates'] as const,
    lists: () => [...queryKeys.candidates.all, 'list'] as const,
    list: (params: ListQueryParams) => [...queryKeys.candidates.lists(), params] as const,
    details: () => [...queryKeys.candidates.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.candidates.details(), id] as const
  },
  jobApplications: {
    all: ['job-applications'] as const,
    mine: (params: ListQueryParams) => [...queryKeys.jobApplications.all, 'mine', params] as const,
    allList: (params: ListQueryParams) => [...queryKeys.jobApplications.all, 'all', params] as const
  },
  users: {
    me: () => ['users', 'me'] as const
  }
} as const;
