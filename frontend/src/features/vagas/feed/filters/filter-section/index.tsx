'use client';

import { actionIcons } from '@/shared/components';
import { useId, useState, type ReactNode } from 'react';
import styles from './filter-section.module.scss';

type FilterSectionProps = {
  title: string;
  /** Quantas opções desta seção estão aplicadas. Zero esconde o contador. */
  activeCount: number;
  /** Resumo do que está selecionado, mostrado com a seção fechada. */
  summary?: string;
  /** Abre a seção na montagem mesmo sem filtros aplicados. */
  defaultOpen?: boolean;
  children: ReactNode;
};

export function FilterSection({ title, activeCount, summary, defaultOpen, children }: FilterSectionProps) {
  const [open, setOpen] = useState(() => defaultOpen === true || activeCount > 0);
  const contentId = useId();

  return (
    <div className={styles.section} data-open={open || undefined}>
      <h3 className={styles.heading}>
        <button
          type="button"
          className={styles.trigger}
          aria-expanded={open}
          aria-controls={contentId}
          onClick={() => setOpen((value) => !value)}
        >
          <span className={styles.title}>{title}</span>

          {activeCount > 0 ? (
            <span className={styles.count}>
              {activeCount}
              <span className="sr-only">{activeCount === 1 ? ' opção selecionada' : ' opções selecionadas'}</span>
            </span>
          ) : null}

          {!open && summary ? <span className={styles.summary}>{summary}</span> : null}

          <actionIcons.expand className={styles.chevron} aria-hidden />
        </button>
      </h3>

      <div id={contentId} className={styles.content} hidden={!open}>
        {children}
      </div>
    </div>
  );
}
