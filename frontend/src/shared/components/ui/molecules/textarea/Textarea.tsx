'use client';

import { cn } from '@/utils/lib';
import type * as React from 'react';
import styles from './Textarea.module.scss';

export type TextareaProps = Omit<React.ComponentProps<'textarea'>, 'className'> & {
  className?: string;
  /** Pinta a borda de erro; a mensagem em si é responsabilidade de quem compõe o campo. */
  invalid?: boolean;
};

export function Textarea({ className, invalid, rows = 4, ...props }: TextareaProps) {
  return (
    <textarea {...props} rows={rows} aria-invalid={invalid || undefined} className={cn(styles.textarea, className)} />
  );
}
