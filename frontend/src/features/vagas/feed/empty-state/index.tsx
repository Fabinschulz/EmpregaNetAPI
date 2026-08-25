'use client';

import { actionIcons, Button } from '@/shared/components';
import { SearchX } from 'lucide-react';
import styles from './empty-state.module.scss';

type FeedEmptyStateProps = {
  hasFilters: boolean;
  onClearFilters: () => void;
};

/**
 * Estado vazio do feed.
 *
 * Distingue os dois motivos: "nenhuma vaga corresponde ao que você filtrou" tem saída (limpar),
 * "não há vagas publicadas" não tem. Tratar os dois com a mesma mensagem deixaria o utilizador
 * achando que o site está quebrado quando na verdade o filtro é que está estreito.
 */
export function FeedEmptyState({ hasFilters, onClearFilters }: FeedEmptyStateProps) {
  return (
    <div className={styles.emptyState}>
      <SearchX className={styles.emptyIcon} aria-hidden />

      <h2 className={styles.emptyTitle}>{hasFilters ? 'Nenhuma vaga com esses filtros' : 'Nenhuma vaga por aqui'}</h2>

      <p className={styles.emptyText}>
        {hasFilters
          ? 'Tente remover algum filtro ou ampliar a faixa salarial e a região.'
          : 'Ainda não há vagas publicadas. Volte em breve - novas oportunidades aparecem toda semana.'}
      </p>

      {hasFilters ? (
        <Button type="button" variant="primary" startIcon={actionIcons.clearFilters} onClick={onClearFilters}>
          Limpar filtros
        </Button>
      ) : null}
    </div>
  );
}
