'use client';

import {
    countActiveJobsFeedFilters,
    defaultJobsFeedFilters,
    jobsFeedFiltersToSearchParams,
    parseJobsFeedFilters,
    type JobsFeedFilters
} from '@/features/vagas/service';
import { useDebouncedDraft } from '@/hooks';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useCallback, useMemo } from 'react';

type ArrayFilterKey = {
  [K in keyof JobsFeedFilters]: JobsFeedFilters[K] extends readonly unknown[] ? K : never;
}[keyof JobsFeedFilters];

const SEARCH_DEBOUNCE_MS = 400;

export function useJobsFeedFilters() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const filters = useMemo(() => parseJobsFeedFilters(searchParams), [searchParams]);

  const write = useCallback(
    (next: JobsFeedFilters, { scrollToTop }: { scrollToTop: boolean }) => {
      const queryString = jobsFeedFiltersToSearchParams(next).toString();
      router.replace(queryString ? `${pathname}?${queryString}` : pathname, { scroll: false });

      if (scrollToTop && typeof window !== 'undefined') {
        window.scrollTo({ top: 0 });
      }
    },
    [pathname, router]
  );

  const commitSearch = useCallback(
    (search: string) => write({ ...filters, search }, { scrollToTop: false }),
    [filters, write]
  );

  const {
    draft: searchDraft,
    setDraft: setSearchDraft,
    isPending: isSearchPending
  } = useDebouncedDraft({ value: filters.search, onCommit: commitSearch, delayMs: SEARCH_DEBOUNCE_MS });

  const updateFilters = useCallback(
    (patch: Partial<JobsFeedFilters>) => {
      write({ ...filters, ...patch }, { scrollToTop: true });
    },
    [filters, write]
  );

  const toggleFilterValue = useCallback(
    <K extends ArrayFilterKey>(key: K, value: JobsFeedFilters[K][number]) => {
      const current = filters[key] as readonly unknown[];
      const next = current.includes(value) ? current.filter((item) => item !== value) : [...current, value];

      write({ ...filters, [key]: next } as JobsFeedFilters, { scrollToTop: true });
    },
    [filters, write]
  );

  const clearAll = useCallback(() => {
    setSearchDraft('');
    write(defaultJobsFeedFilters, { scrollToTop: true });
  }, [setSearchDraft, write]);

  return {
    filters,
    searchDraft,
    onSearchChange: setSearchDraft,
    updateFilters,
    toggleFilterValue,
    clearAll,
    activeCount: countActiveJobsFeedFilters(filters),
    isSearchPending
  };
}

export type JobsFeedFiltersController = ReturnType<typeof useJobsFeedFilters>;
