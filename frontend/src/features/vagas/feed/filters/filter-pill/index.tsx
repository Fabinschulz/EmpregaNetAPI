'use client';

import { actionIcons, Popover, PopoverContent, PopoverTrigger } from '@/shared/components';
import type { ReactNode } from 'react';
import styles from './filter-pill.module.scss';

type FilterPillProps = {
  label: string;
  activeCount: number;
  children: ReactNode;
};

export function FilterPill({ label, activeCount, children }: FilterPillProps) {
  const isActive = activeCount > 0;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <button type="button" className={styles.pill} data-active={isActive || undefined}>
          {label}
          {isActive ? <span className={styles.count}>{activeCount}</span> : null}
          <actionIcons.expand className={styles.chevron} aria-hidden />
        </button>
      </PopoverTrigger>

      <PopoverContent align="start" className={styles.panel}>
        {children}
      </PopoverContent>
    </Popover>
  );
}

type TogglePillProps = {
  label: string;
  pressed: boolean;
  onToggle: () => void;
};

export function TogglePill({ label, pressed, onToggle }: TogglePillProps) {
  return (
    <button
      type="button"
      className={styles.pill}
      aria-pressed={pressed}
      data-active={pressed || undefined}
      onClick={onToggle}
    >
      {label}
    </button>
  );
}
