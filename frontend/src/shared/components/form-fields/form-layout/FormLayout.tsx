'use client';

import { cn } from '@/utils';
import type { ReactNode } from 'react';
import styles from './form-layout.module.scss';

type FormLayoutProps = {
  children: ReactNode;
  className?: string;
};

/** Contêiner do formulário: grid vertical com largura máxima e espaçamento padrão. */
export function FormGrid({ children, className }: FormLayoutProps) {
  return <div className={cn(styles.grid, className)}>{children}</div>;
}

/** Linha de campos lado a lado (duas colunas; colapsa para uma em telas estreitas). */
export function FormRow({ children, className }: FormLayoutProps) {
  return <div className={cn(styles.row, className)}>{children}</div>;
}

type FormSectionProps = FormLayoutProps & {
  /** Título da seção, renderizado como `<legend>` do agrupamento. */
  title: string;
};

/** Agrupamento semântico de campos relacionados (`fieldset` + `legend`). */
export function FormSection({ title, children, className }: FormSectionProps) {
  return (
    <fieldset className={cn(styles.section, className)}>
      <legend className={styles.legend}>{title}</legend>
      <div className={styles.sectionBody}>{children}</div>
    </fieldset>
  );
}

/** Rodapé com as ações do formulário (ex.: salvar). */
export function FormActions({ children, className }: FormLayoutProps) {
  return <div className={cn(styles.actions, className)}>{children}</div>;
}
