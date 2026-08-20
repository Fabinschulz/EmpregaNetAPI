import { cn } from '@/shared/utils/lib';
import type { ReactNode } from 'react';
import styles from './CardSectionLabel.module.scss';

export type CardSectionLabelProps = {
  children: ReactNode;
  /** `p` quando o rótulo é só visual e não estrutura o documento. */
  as?: 'h2' | 'h3' | 'p';
  className?: string;
};

export function CardSectionLabel({ children, as: Tag = 'p', className }: CardSectionLabelProps) {
  return <Tag className={cn(styles.label, className)}>{children}</Tag>;
}
