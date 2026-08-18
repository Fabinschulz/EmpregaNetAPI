'use client';

import { Spinner } from '@/shared/components';
import { Search, X } from 'lucide-react';
import styles from './search-bar.module.scss';

type FeedSearchBarProps = {
  value: string;
  onChange: (value: string) => void;
  isPending: boolean;
};

export function FeedSearchBar({ value, onChange, isPending }: FeedSearchBarProps) {
  return (
    <form className={styles.searchBar} role="search" onSubmit={(event) => event.preventDefault()}>
      <label className="sr-only" htmlFor="jobs-feed-search">
        Buscar vagas por cargo, empresa, tecnologia ou cidade
      </label>

      <Search className={styles.searchIcon} aria-hidden />

      <input
        id="jobs-feed-search"
        type="search"
        className={styles.searchInput}
        placeholder="Cargo, empresa, tecnologia ou cidade..."
        value={value}
        autoComplete="off"
        onChange={(event) => onChange(event.target.value)}
      />

      <span className={styles.searchStatus}>
        {isPending ? <Spinner size="sm" label="Atualizando resultados" /> : null}

        {value ? (
          <button type="button" className={styles.searchClear} onClick={() => onChange('')} aria-label="Limpar busca">
            <X aria-hidden />
          </button>
        ) : null}
      </span>
    </form>
  );
}
