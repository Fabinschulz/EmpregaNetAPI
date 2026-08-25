'use client';

import { actionIcons, Button, IconButton } from '@/shared/components';
import type { JobVocabularyResponse } from '@/features/vagas/service';
import * as Dialog from '@radix-ui/react-dialog';
import { useState } from 'react';
import { FeedFiltersForm } from '../filters-form';
import styles from '../filters.module.scss';
import type { JobsFeedFiltersController } from '../use-jobs-feed-filters';

type FeedFiltersDrawerProps = {
  controller: JobsFeedFiltersController;
  vocabulary: JobVocabularyResponse;
  totalItems: number;
};

export function FeedFiltersDrawer({ controller, vocabulary, totalItems }: FeedFiltersDrawerProps) {
  const [open, setOpen] = useState(false);
  const { activeCount, clearAll } = controller;

  const resultLabel = `${totalItems} ${totalItems === 1 ? 'vaga' : 'vagas'}`;

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger asChild>
        <Button type="button" variant="outline" size="sm" className={styles.drawerTrigger}>
          <actionIcons.filter aria-hidden />
          Todos os filtros
          {activeCount > 0 ? <span className={styles.drawerCount}>{activeCount}</span> : null}
        </Button>
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Overlay className={styles.overlay} />

        <Dialog.Content className={styles.drawer} aria-describedby={undefined}>
          <div className={styles.drawerGrabber} aria-hidden />

          <div className={styles.drawerHeader}>
            <div className={styles.drawerHeading}>
              <Dialog.Title className={styles.panelTitle}>Filtros</Dialog.Title>

              <p className={styles.drawerSubtitle}>
                {activeCount > 0
                  ? `${activeCount} ${activeCount === 1 ? 'filtro aplicado' : 'filtros aplicados'}`
                  : 'Nenhum filtro aplicado'}
              </p>
            </div>

            <Dialog.Close asChild>
              <IconButton icon={actionIcons.close} label="Fechar filtros" showTooltip={false} />
            </Dialog.Close>
          </div>

          <div className={styles.drawerBody}>
            <FeedFiltersForm controller={controller} vocabulary={vocabulary} />
          </div>

          <div className={styles.drawerFooter}>
            <p className={styles.drawerLiveStatus} role="status" aria-live="polite">
              {resultLabel} {totalItems === 1 ? 'encontrada' : 'encontradas'}
            </p>

            <Button
              type="button"
              variant="outline"
              className={styles.drawerClear}
              startIcon={actionIcons.clearFilters}
              onClick={clearAll}
              disabled={activeCount === 0}
            >
              Limpar filtros
            </Button>

            <Dialog.Close asChild>
              <Button type="button" variant="primary" className={styles.drawerSubmit} startIcon={actionIcons.confirm}>
                Ver {resultLabel}
              </Button>
            </Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
