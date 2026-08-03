'use client';

import { useAuth } from '@/context';
import { keepPreviousData, useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';
import { fetchJobFeedInteractions, fetchJobVocabulary, fetchJobsFeed } from './jobs-feed-api';
import { jobsFeedKeys } from './jobs-feed-keys';
import {
  JOBS_FEED_PAGE_SIZE,
  jobsFeedFiltersToApiParams,
  jobsFeedFiltersToQueryString,
  type JobFeedItemDto,
  type JobVocabularyDto,
  type JobsFeedFilters,
  type JobsFeedResponseDto
} from './jobs-feed-schema';

type UseJobsFeedQueryOptions = {
  initialPage?: JobsFeedResponseDto;
  initialFilters?: JobsFeedFilters;
};

export function useJobsFeedQuery(
  filters: JobsFeedFilters,
  { initialPage, initialFilters }: UseJobsFeedQueryOptions = {}
) {
  // Só semeia quando a chave atual corresponde exatamente ao que o servidor renderizou.
  const seedsCurrentFilters =
    initialPage !== undefined &&
    initialFilters !== undefined &&
    jobsFeedFiltersToQueryString(initialFilters) === jobsFeedFiltersToQueryString(filters);

  const query = useInfiniteQuery({
    queryKey: jobsFeedKeys.list(filters),
    queryFn: ({ pageParam }) => fetchJobsFeed(jobsFeedFiltersToApiParams(filters, pageParam, JOBS_FEED_PAGE_SIZE)),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const totalPages = lastPage.totalPages ?? 0;
      const nextPage = allPages.length + 1;
      return nextPage <= totalPages ? nextPage : undefined;
    },
    placeholderData: keepPreviousData,
    initialData: seedsCurrentFilters ? { pages: [initialPage], pageParams: [1] } : undefined
  });

  const jobs = useMemo<JobFeedItemDto[]>(() => query.data?.pages.flatMap((page) => page.data) ?? [], [query.data]);

  const totalItems = query.data?.pages[0]?.totalItems ?? 0;

  return { ...query, jobs, totalItems };
}

export function useJobVocabularyQuery(initialData?: JobVocabularyDto) {
  return useQuery({
    queryKey: jobsFeedKeys.vocabulary(),
    queryFn: fetchJobVocabulary,
    staleTime: Infinity,
    gcTime: Infinity,
    refetchOnWindowFocus: false,
    initialData
  });
}

export function useJobFeedInteractionsQuery(jobIds: number[]) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: jobsFeedKeys.interactions(jobIds),
    queryFn: () => fetchJobFeedInteractions(jobIds),
    enabled: isAuthenticated && jobIds.length > 0,
    placeholderData: keepPreviousData,
    refetchOnWindowFocus: false
  });
}
