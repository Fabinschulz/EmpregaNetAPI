'use client';

import { Label } from '@/components';
import { useFormContext } from '@/context';
import { Eye, EyeOff } from 'lucide-react';
import * as React from 'react';
import { cn, getFieldErrorMessage } from '@/utils';
import styles from './input.module.scss';

export type InputFieldProps = Omit<React.ComponentProps<'input'>, 'className'> & {
  name: string;
  label?: string;
  className?: string;
  startIcon?: React.FC<React.SVGProps<SVGSVGElement>>;
  endIcon?: React.FC<React.SVGProps<SVGSVGElement>>;
  onEndIconClick?: () => void;
  /** Rótulo acessível do botão de end-icon quando ele dispara uma ação. */
  endIconLabel?: string;
  error?: string;
  onFieldChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  hint?: string | null;
  /**
   * Habilita o botão de mostrar/ocultar senha. Automático para `type="password"`;
   * passe `false` para desabilitar.
   */
  showPasswordToggle?: boolean;
};

export const InputField: React.FC<InputFieldProps> = ({
  name,
  label,
  required,
  placeholder,
  type = 'text',
  className,
  startIcon: StartIcon,
  endIcon: EndIcon,
  onEndIconClick,
  endIconLabel,
  error: errorProp,
  onFieldChange,
  hint,
  showPasswordToggle,
  ...props
}) => {
  const { register, validationErrors, readOnly } = useFormContext();
  const fromForm = getFieldErrorMessage(name, validationErrors);
  const errorsMessage = errorProp ?? fromForm;
  const labelText = required && label ? `${label} *` : label;

  const isPassword = type === 'password';
  const canTogglePassword = isPassword && (showPasswordToggle ?? true) && !EndIcon;
  const [passwordVisible, setPasswordVisible] = React.useState(false);
  const resolvedType = isPassword && passwordVisible ? 'text' : type;

  const errorId = `${name}-error`;
  const hintId = `${name}-hint`;
  const describedBy = errorsMessage ? errorId : hint ? hintId : undefined;

  const { ref, onChange: regOnChange, ...regRest } = register(name);
  const { onChange: propOnChange, ...restProps } = props;

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? (
        <Label htmlFor={name} className={styles.label}>
          {labelText}
        </Label>
      ) : null}

      <div className={styles.row}>
        {StartIcon ? (
          <span className={styles.iconBtn} aria-hidden>
            <StartIcon width={18} height={18} />
          </span>
        ) : null}
        <input
          {...restProps}
          {...regRest}
          id={name}
          ref={ref}
          type={resolvedType}
          placeholder={placeholder}
          disabled={!!readOnly}
          aria-invalid={!!errorsMessage}
          aria-describedby={describedBy}
          className={styles.input}
          onChange={(e) => {
            void regOnChange(e);
            propOnChange?.(e);
            onFieldChange?.(e);
          }}
        />
        {canTogglePassword ? (
          <button
            type="button"
            className={styles.iconBtn}
            onClick={() => setPasswordVisible((visible) => !visible)}
            aria-label={passwordVisible ? 'Ocultar senha' : 'Mostrar senha'}
            aria-pressed={passwordVisible}
          >
            {passwordVisible ? (
              <EyeOff width={18} height={18} aria-hidden />
            ) : (
              <Eye width={18} height={18} aria-hidden />
            )}
          </button>
        ) : EndIcon && onEndIconClick ? (
          <button
            type="button"
            className={styles.iconBtn}
            onClick={onEndIconClick}
            aria-label={endIconLabel ?? 'Ação do campo'}
          >
            <EndIcon width={18} height={18} aria-hidden />
          </button>
        ) : EndIcon ? (
          <span className={styles.iconBtn} aria-hidden>
            <EndIcon width={18} height={18} />
          </span>
        ) : null}
      </div>

      {errorsMessage ? (
        <span id={errorId} className={styles.error} role="alert">
          {errorsMessage}
        </span>
      ) : hint ? (
        <span id={hintId} className={styles.hint}>
          {hint}
        </span>
      ) : null}
    </div>
  );
};
