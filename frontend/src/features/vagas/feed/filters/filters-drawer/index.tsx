'use client';

import { Button } from '@/components';
import type { JobVocabularyDto } from '@/features/vagas/service';
import * as Dialog from '@radix-ui/react-dialog';
import { SlidersHorizontal, X } from 'lucide-react';
import { useState } from 'react';
import { FeedFiltersForm } from '../filters-form';
import styles from '../filters.module.scss';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';

type FeedFiltersDrawerProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyDto;
  totalItems: number;
};

export function FeedFiltersDrawer({ controller, vocabulary, totalItems }: FeedFiltersDrawerProps) {
  const [open, setOpen] = useState(false);
  const { activeCount, clearAll } = controller;

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger asChild>
        <Button type="button" variant="outline" size="sm" className={styles.drawerTrigger}>
          <SlidersHorizontal aria-hidden />
          Todos os filtros
          {activeCount > 0 ? <span className={styles.drawerCount}>{activeCount}</span> : null}
        </Button>
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Overlay className={styles.overlay} />

        <Dialog.Content className={styles.drawer} aria-describedby={undefined}>
          <div className={styles.drawerHeader}>
            <Dialog.Title className={styles.panelTitle}>Filtros</Dialog.Title>

            <Dialog.Close asChild>
              <Button type="button" variant="ghost" size="icon" aria-label="Fechar filtros">
                <X aria-hidden />
              </Button>
            </Dialog.Close>
          </div>

          <div className={styles.drawerBody}>
            <FeedFiltersForm controller={controller} vocabulary={vocabulary} />
          </div>

          <div className={styles.drawerFooter}>
            <Button type="button" variant="outline" onClick={clearAll} disabled={activeCount === 0}>
              Limpar filtros
            </Button>

            <Dialog.Close asChild>
              <Button type="button" variant="primary">
                Ver {totalItems} {totalItems === 1 ? 'vaga' : 'vagas'}
              </Button>
            </Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
