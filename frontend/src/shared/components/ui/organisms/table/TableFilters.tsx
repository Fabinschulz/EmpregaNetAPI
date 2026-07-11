'use client';

import { cn } from '@/utils/lib';
import * as React from 'react';
import styles from './TableFilters.module.scss';

export type TableFiltersProps = {
  /** Título do painel de busca (ex.: "Buscar vagas"). */
  title: string;
  /** Descrição curta opcional exibida abaixo do título. */
  description?: string;
  /** Ações do painel (ex.: botão "Limpar filtros"), alinhadas à direita do título. */
  actions?: React.ReactNode;
  /** O formulário de filtros em si (campos + botão de busca). */
  children: React.ReactNode;
  className?: string;
};

/**
 * Painel padrão de filtros de tabela: cabeçalho com título (e ações opcionais)
 * seguido do formulário de filtros. Garante que toda tela de listagem apresente
 * a busca com a mesma aparência e estrutura.
 */
export function TableFilters({ title, description, actions, children, className }: TableFiltersProps) {
  return (
    <section className={cn(styles.root, className)} aria-label={title}>
      <header className={styles.header}>
        <div>
          <h2 className={styles.title}>{title}</h2>
          {description ? <p className={styles.description}>{description}</p> : null}
        </div>
        {actions ? <div className={styles.actions}>{actions}</div> : null}
      </header>
      <div className={styles.content}>{children}</div>
    </section>
  );
}
