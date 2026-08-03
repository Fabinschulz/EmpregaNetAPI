import { jobsFeedFiltersToQueryString, type JobsFeedFilters } from './jobs-feed-schema';

export const jobsFeedKeys = {
  all: ['jobs-feed'] as const,
  lists: () => [...jobsFeedKeys.all, 'list'] as const,
  list: (filters: JobsFeedFilters) => [...jobsFeedKeys.lists(), jobsFeedFiltersToQueryString(filters)] as const,
  vocabulary: () => [...jobsFeedKeys.all, 'vocabulary'] as const,
  interactions: (jobIds: readonly number[]) =>
    [...jobsFeedKeys.all, 'interactions', [...jobIds].sort((a, b) => a - b).join(',')] as const
};
