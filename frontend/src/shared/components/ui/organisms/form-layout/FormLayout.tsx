'use client';

import { cn } from '@/shared/utils';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import type { ReactNode } from 'react';
import { Button } from '../../atoms/button/Button';
import styles from './FormLayout.module.scss';

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

type FormActionsProps = FormLayoutProps & {
  backHref?: string;
  backLabel?: string;
};

/** Rodapé com as ações do formulário. */
export function FormActions({ children, className, backHref, backLabel = 'Voltar' }: FormActionsProps) {
  return (
    <div className={cn(styles.actions, className)}>
      {backHref ? (
        <Button type="button" variant="outline" className={styles.back} asChild>
          <Link href={backHref}>
            <ArrowLeft aria-hidden />
            {backLabel}
          </Link>
        </Button>
      ) : null}
      {children}
    </div>
  );
}
