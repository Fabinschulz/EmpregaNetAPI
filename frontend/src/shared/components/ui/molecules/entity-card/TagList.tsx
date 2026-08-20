import { cn } from '@/shared/utils/lib';
import type { CardTag } from './card-tags';
import styles from './TagList.module.scss';

export type TagListProps = {
  tags: readonly CardTag[];
  /** Etiquetas visíveis antes do `+N`. Sem limite quando omitido. */
  max?: number;
  ariaLabel?: string;
  className?: string;
};


export function TagList({ tags, max, ariaLabel, className }: TagListProps) {
  if (tags.length === 0) return null;

  const limit = max ?? tags.length;
  const visible = tags.slice(0, limit);
  const hidden = tags.slice(limit);
  const hiddenLabels = hidden.map((tag) => tag.label).join(', ');

  return (
    <ul className={cn(styles.list, className)} aria-label={ariaLabel}>
      {visible.map((tag) => (
        <li key={tag.key} className={cn(styles.tag, tag.tone === 'accent' && styles.accent)}>
          {tag.label}
        </li>
      ))}

      {hidden.length > 0 ? (
        <li className={styles.more} title={hiddenLabels}>
          +{hidden.length}
          <span className="sr-only"> e mais: {hiddenLabels}</span>
        </li>
      ) : null}
    </ul>
  );
}
