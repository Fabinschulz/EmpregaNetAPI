'use client';

import { cn } from '@/utils/lib';
import { Eye, EyeOff } from 'lucide-react';
import * as React from 'react';
import styles from './Input.module.scss';

export type InputProps = Omit<React.ComponentProps<'input'>, 'className'> & {
  className?: string;
  /** Pinta a borda de erro; a mensagem em si é responsabilidade de quem compõe o campo. */
  invalid?: boolean;
  /** Ícone ou conteúdo decorativo antes do controle. */
  startAdornment?: React.ReactNode;
  /** Ícone, botão ou conteúdo depois do controle. Ignorado quando o toggle de senha aparece. */
  endAdornment?: React.ReactNode;
  /** Botão de mostrar/ocultar senha. Automático para `type="password"`; passe `false` para desligar. */
  passwordToggle?: boolean;
};

/**
 * Controle de texto da biblioteca de UI: só o `<input>`, os adornos e os estados visuais.
 *
 * <para>Rótulo, erro e dica ficam com quem compõe (ver `FormField`), para que o mesmo
 * controle sirva dentro e fora de um formulário.</para>
 */
export function Input({
  className,
  invalid,
  startAdornment,
  endAdornment,
  passwordToggle,
  type = 'text',
  ...props
}: InputProps) {
  const isPassword = type === 'password';
  const canToggle = isPassword && (passwordToggle ?? true) && !endAdornment;
  const [passwordVisible, setPasswordVisible] = React.useState(false);

  return (
    <div className={cn(styles.row, className)}>
      {startAdornment ? (
        <span className={styles.adornment} aria-hidden>
          {startAdornment}
        </span>
      ) : null}

      <input
        {...props}
        type={isPassword && passwordVisible ? 'text' : type}
        aria-invalid={invalid || undefined}
        className={styles.input}
      />

      {canToggle ? (
        <button
          type="button"
          className={styles.adornment}
          onClick={() => setPasswordVisible((visible) => !visible)}
          aria-label={passwordVisible ? 'Ocultar senha' : 'Mostrar senha'}
          aria-pressed={passwordVisible}
        >
          {passwordVisible ? <EyeOff width={18} height={18} aria-hidden /> : <Eye width={18} height={18} aria-hidden />}
        </button>
      ) : (
        endAdornment
      )}
    </div>
  );
}
