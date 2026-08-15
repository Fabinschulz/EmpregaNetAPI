'use client';

import { Label } from '@/components/ui';
import { cn } from '@/utils';
import type { ReactNode } from 'react';
import type { FormFieldIds } from './use-form-field';
import styles from './form-field.module.scss';

export type FormFieldProps = {
  ids: FormFieldIds;
  label?: string;
  required?: boolean;
  error?: string;
  hint?: string | null;
  className?: string;
  /**
   * `false` para controles sem elemento focável nativo (combobox montado sobre `div`/`button`),
   * que se ligam ao rótulo por `aria-labelledby` em vez de `htmlFor`.
   */
  labelFor?: boolean;
  children: ReactNode;
};

/**
 * Casca partilhada por todos os campos: rótulo, mensagem de erro e dica.
 *
 * <para>Nenhum campo desenha isto por conta própria - mudar a acessibilidade do erro
 * ou o marcador de obrigatório é uma edição só, aqui.</para>
 */
export function FormField({ ids, label, required, error, hint, className, labelFor = true, children }: FormFieldProps) {
  return (
    <div className={cn(styles.field, className)}>
      {label ? (
        labelFor ? (
          <Label id={ids.label} htmlFor={ids.control}>
            {required ? `${label} *` : label}
          </Label>
        ) : (
          /* Sem `htmlFor` um `<label>` não rotula nada: o combobox liga-se por `aria-labelledby`. */
          <span id={ids.label} className={styles.label}>
            {required ? `${label} *` : label}
          </span>
        )
      ) : null}

      {children}

      {error ? (
        <span id={ids.error} className={styles.error} role="alert">
          {error}
        </span>
      ) : hint ? (
        <span id={ids.hint} className={styles.hint}>
          {hint}
        </span>
      ) : null}
    </div>
  );
}
