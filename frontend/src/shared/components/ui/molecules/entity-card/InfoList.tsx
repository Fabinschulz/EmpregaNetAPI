import { cn } from '@/shared/utils/lib';
import type { LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import styles from './InfoList.module.scss';

export type InfoListProps = {
  ariaLabel?: string;
  children: ReactNode;
  className?: string;
};

export function InfoList({ ariaLabel, children, className }: InfoListProps) {
  return (
    <ul className={cn(styles.list, className)} aria-label={ariaLabel}>
      {children}
    </ul>
  );
}

export type InfoItemProps = {
  icon: LucideIcon;
  /** Valor já formatado; este componente não formata nada. */
  children: ReactNode;
  /** Sobe o peso e a cor, para o dado que decide a leitura. */
  strong?: boolean;
  /** Significado do ícone para quem não o vê (ex.: "Localização"). */
  srLabel?: string;
  /** Texto completo, quando o valor pode ser cortado por reticências. */
  title?: string;
  className?: string;
};

export function InfoItem({ icon: Icon, children, strong, srLabel, title, className }: InfoItemProps) {
  return (
    <li className={cn(styles.item, strong && styles.strong, className)}>
      <Icon className={styles.icon} aria-hidden />
      {srLabel ? <span className="sr-only">{srLabel}: </span> : null}
      <span className={styles.text} title={title}>
        {children}
      </span>
    </li>
  );
}
