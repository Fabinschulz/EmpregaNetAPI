'use client';

import { useFormContext } from '@/shared/context';
import { cn } from '@/shared/utils';
import type { LucideIcon } from 'lucide-react';
import Link from 'next/link';
import type { CSSProperties, ReactNode } from 'react';
import { FormSubmitButton } from '../../../common/form-submit-btn';
import { Button } from '../../atoms/button';
import { actionIcons } from '../../icons';
import { PageHeader } from '../../molecules/page-header';
import styles from './FormLayout.module.scss';

export type FormColumns = 1 | 2 | 3 | 4;
export type FormSpan = FormColumns | 'full';

type FormLayoutProps = {
  children: ReactNode;
  className?: string;
};

const columnsStyle = (cols: FormColumns): CSSProperties =>
  ({ '--form-cols': cols, '--form-cols-md': Math.min(cols, 2) }) as CSSProperties;

export type FormGridProps = FormLayoutProps & {
  narrow?: boolean;
};

export function FormGrid({ narrow, children, className }: FormGridProps) {
  return <div className={cn(styles.grid, narrow && styles.gridNarrow, className)}>{children}</div>;
}

export function FormNotice({ children, className }: FormLayoutProps) {
  return <div className={cn(styles.notice, className)}>{children}</div>;
}

export type FormRowProps = FormLayoutProps & {
  cols?: FormColumns;
};

/**
 * Linha de campos lado a lado. Cada filho direto ocupa uma coluna; para dar mais
 * largura a um campo, envolva-o em {@link FormCol}.
 *
 * <para>Dentro de uma {@link FormSection} não é necessária: o corpo da seção já é
 * um grid próprio.</para>
 */
export function FormRow({ cols = 2, children, className }: FormRowProps) {
  return (
    <div className={cn(styles.row, className)} style={columnsStyle(cols)}>
      {children}
    </div>
  );
}

export type FormColProps = FormLayoutProps & {
  /** Nº de colunas que o campo ocupa (ex.: 2) ou 'full' para a linha inteira. */
  span?: FormSpan;
};

/**
 * Wrapper "OPCIONAL" de largura dentro de {@link FormRow} ou {@link FormSection}:
 * campos sem wrapper ocupam uma coluna. Use para o que precisa de mais espaço
 * (logradouro, descrição) sem quebrar o grid da seção.
 *
 * <para>O `span` numérico é proporcional: quando o grid encolhe, o campo volta a uma
 * célula. Para largura total em qualquer tela - textarea, editor - use `span="full"`.</para>
 */
export function FormCol({ span = 1, children, className }: FormColProps) {
  const isFull = span === 'full';

  return (
    <div
      className={cn(styles.col, isFull && styles.colFull, className)}
      style={isFull ? undefined : ({ '--form-span': span } as CSSProperties)}
    >
      {children}
    </div>
  );
}

export type FormSectionProps = FormLayoutProps & {
  /** Título da seção, renderizado como `<legend>` do agrupamento. */
  title: string;
  /** Texto curto de apoio, quando o título sozinho não explica o agrupamento. */
  description?: ReactNode;
  /** Colunas do grid da seção em desktop (padrão 2). */
  cols?: FormColumns;
};

export function FormSection({ title, description, cols = 2, children, className }: FormSectionProps) {
  return (
    <fieldset className={cn(styles.section, className)}>
      <legend className={styles.legend}>{title}</legend>
      {description ? <p className={styles.sectionDescription}>{description}</p> : null}
      <div className={styles.row} style={columnsStyle(cols)}>
        {children}
      </div>
    </fieldset>
  );
}

export type FormHeaderProps = {
  title?: ReactNode;
  description?: ReactNode;
  backHref?: string;
  backLabel?: string;
  submitLabel: string;
  /** Ícone da ação primária (padrão: disquete). Dá lugar ao `Spinner` durante o envio. */
  submitIcon?: LucideIcon;
  submitDisabled?: boolean;
  /** Ações secundárias do formulário (ex.: "Encerrar vaga"), entre Voltar e a ação primária. */
  children?: ReactNode;
  /** Mantém o cabeçalho colado ao topo ao rolar (padrão: sim, de tablet para cima). */
  sticky?: boolean;
  className?: string;
};

/**
 * Cabeçalho do formulário: título à esquerda, ações à direita.
 *
 * <para>As ações vivem aqui, e não num rodapé, para ficarem alcançáveis sem rolar até
 * o fim de um formulário longo. Como é renderizado dentro do `FormProvider`, o botão
 * primário é o `submit` do próprio `<form>` - nada de `form="id"` nem de handler avulso.</para>
 */
export function FormHeader({
  title,
  description,
  backHref,
  backLabel = 'Voltar',
  submitLabel,
  submitIcon,
  submitDisabled,
  children,
  sticky = true,
  className
}: FormHeaderProps) {
  const { readOnly, reset } = useFormContext();

  const actions = (
    <>
      {backHref ? (
        <Button type="button" variant="outline" asChild>
          <Link href={backHref} onClick={() => setTimeout(() => reset())}>
            <actionIcons.back aria-hidden />
            {backLabel}
          </Link>
        </Button>
      ) : null}
      {children}

      <FormSubmitButton
        variant="primary"
        icon={submitIcon ?? actionIcons.save}
        disabled={!!submitDisabled || !!readOnly}
      >
        {submitLabel}
      </FormSubmitButton>
    </>
  );

  if (!title) {
    return <div className={cn(styles.actionsBar, className)}>{actions}</div>;
  }

  return (
    <PageHeader
      className={cn(styles.header, sticky && styles.headerSticky, className)}
      title={title}
      description={description}
      actions={actions}
    />
  );
}

export type FormActionsProps = FormLayoutProps & {
  backHref?: string;
  backLabel?: string;
};

export function FormActions({ children, className, backHref, backLabel = 'Voltar' }: FormActionsProps) {
  const { reset } = useFormContext();

  return (
    <div className={cn(styles.actions, className)}>
      {backHref ? (
        <Button type="button" variant="outline" className={styles.back} asChild>
          <Link href={backHref} onClick={() => setTimeout(() => reset())}>
            <actionIcons.back aria-hidden />
            {backLabel}
          </Link>
        </Button>
      ) : null}
      {children}
    </div>
  );
}
