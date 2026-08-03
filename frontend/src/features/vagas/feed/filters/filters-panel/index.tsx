'use client';

import { Button } from '@/components';
import type { JobVocabularyDto } from '@/features/vagas/service';
import { X } from 'lucide-react';
import { FeedFiltersForm } from '../filters-form';
import styles from '../filters.module.scss';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';

type FeedFiltersPanelProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyDto;
};

export function FeedFiltersPanel({ controller, vocabulary }: FeedFiltersPanelProps) {
  const { activeCount, clearAll } = controller;

  return (
    <aside className={styles.panel} aria-label="Filtros de vagas">
      <div className={styles.panelHeader}>
        <h2 className={styles.panelTitle}>Filtros</h2>

        {activeCount > 0 ? (
          <Button type="button" variant="ghost" size="sm" onClick={clearAll}>
            <X aria-hidden />
            Limpar
          </Button>
        ) : null}
      </div>

      <FeedFiltersForm controller={controller} vocabulary={vocabulary} />
    </aside>
  );
}
