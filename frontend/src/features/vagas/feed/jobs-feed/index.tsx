'use client';

import { Alert, Button, PageHeader } from '@/shared/components';
import {
  emptyJobVocabulary,
  useJobFeedInteractionsQuery,
  useJobVocabularyQuery,
  useJobsFeedQuery,
  type JobVocabularyResponse,
  type JobsFeedResponse
} from '@/features/vagas/service';
import type { JobsFeedFilters } from '../filters/jobs-feed-filters';
import { useInfiniteScroll } from '@/shared/hooks';
import { RefreshCw } from 'lucide-react';
import { useMemo } from 'react';
import { FeedActiveChips } from '../active-chips';
import { FeedEmptyState } from '../empty-state';
import { FeedFilterPills, useJobsFeedFilters } from '../filters';
import { JobCard, JobCardSkeletonList } from '../job-card';
import { FeedSearchBar } from '../search-bar';
import { FeedSortSelect } from '../sort-select';
import styles from './jobs-feed.module.scss';

type JobsFeedProps = {
  initialPage?: JobsFeedResponse;
  initialFilters?: JobsFeedFilters;
  initialVocabulary?: JobVocabularyResponse;
};

export function JobsFeed({ initialPage, initialFilters, initialVocabulary }: JobsFeedProps) {
  const controller = useJobsFeedFilters();
  const { filters, searchDraft, onSearchChange, updateFilters, clearAll, activeCount, isSearchPending } = controller;

  const { jobs, totalItems, isPending, isFetching, isFetchingNextPage, hasNextPage, fetchNextPage, isError, refetch } =
    useJobsFeedQuery(filters, { initialPage, initialFilters });

  const { data: vocabulary } = useJobVocabularyQuery(initialVocabulary);
  const resolvedVocabulary = vocabulary ?? emptyJobVocabulary;

  const visibleJobIds = useMemo(() => jobs.map((job) => job.id), [jobs]);
  const { data: interactions } = useJobFeedInteractionsQuery(visibleJobIds);

  const appliedJobIds = useMemo(() => new Set(interactions?.appliedJobIds ?? []), [interactions]);

  const sentinelRef = useInfiniteScroll({
    hasMore: Boolean(hasNextPage),
    isLoading: isFetchingNextPage,
    onLoadMore: () => void fetchNextPage()
  });

  const showEmptyState = !isPending && !isError && jobs.length === 0;

  return (
    <div className={styles.page}>
      <PageHeader title="Vagas" description="Encontre a sua próxima oportunidade." />

      <div className={styles.toolbar}>
        <FeedSearchBar value={searchDraft} onChange={onSearchChange} isPending={isSearchPending} />
        <FeedFilterPills controller={controller} vocabulary={resolvedVocabulary} totalItems={totalItems} />
      </div>

      <FeedActiveChips controller={controller} />

      <div className={styles.layout}>
        <section className={styles.results} aria-label="Resultados de vagas">
          <div className={styles.resultsHeader}>
            <p className={styles.resultsCount} role="status" aria-live="polite">
              {isPending
                ? 'Carregando vagas...'
                : `${totalItems} ${totalItems === 1 ? 'vaga encontrada' : 'vagas encontradas'}`}
            </p>

            <FeedSortSelect
              value={filters.sort}
              onChange={(sort) => updateFilters({ sort })}
              hasSearch={Boolean(filters.search.trim())}
            />
          </div>

          {isError ? (
            <Alert variant="destructive" title="Não foi possível carregar as vagas">
              <p>Verifique a sua conexão e tente novamente.</p>
              <Button
                type="button"
                variant="outline"
                size="sm"
                startIcon={RefreshCw}
                onClick={() => void refetch()}
                disabled={isFetching}
              >
                Tentar novamente
              </Button>
            </Alert>
          ) : null}

          {showEmptyState ? <FeedEmptyState hasFilters={activeCount > 0} onClearFilters={clearAll} /> : null}

          {!isError && (isPending || jobs.length > 0) ? (
            <div
              className={styles.feed}
              role="feed"
              aria-busy={isPending || isFetchingNextPage}
              aria-label="Vagas disponíveis"
            >
              {isPending ? (
                <JobCardSkeletonList count={4} />
              ) : (
                jobs.map((job, index) => (
                  <JobCard
                    key={job.id}
                    className={styles.feedItem}
                    job={job}
                    hasApplied={appliedJobIds.has(job.id)}
                    position={index + 1}
                    totalItems={totalItems}
                  />
                ))
              )}

              {isFetchingNextPage ? <JobCardSkeletonList count={2} /> : null}
            </div>
          ) : null}

          {hasNextPage ? <div ref={sentinelRef} className={styles.sentinel} aria-hidden /> : null}
          {!hasNextPage && jobs.length > 0 ? (
            <p className={styles.feedEnd}>Você chegou ao fim dos resultados.</p>
          ) : null}
        </section>
      </div>
    </div>
  );
}
