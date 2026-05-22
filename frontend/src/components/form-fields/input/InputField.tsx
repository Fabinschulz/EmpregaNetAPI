'use client';

import { Label } from '@/components';
import { useFormContext } from '@/context';
import type React from 'react';
import { cn, getFieldErrorMessage } from 'src/utils';
import styles from './input.module.scss';

export type InputFieldProps = Omit<React.ComponentProps<'input'>, 'className'> & {
  name: string;
  label?: string;
  className?: string;
  startIcon?: React.FC<React.SVGProps<SVGSVGElement>>;
  endIcon?: React.FC<React.SVGProps<SVGSVGElement>>;
  onEndIconClick?: () => void;
  error?: string;
  /** @deprecated use `onFieldChange` */
  onFielChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onFieldChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  hint?: string | null;
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
  error: errorProp,
  onFielChange,
  onFieldChange,
  hint,
  ...props
}) => {
  const { register, validationErrors, readOnly } = useFormContext();
  const fromForm = getFieldErrorMessage(name, validationErrors);
  const errorsMessage = errorProp ?? fromForm;
  const labelText = required && label ? `${label} *` : label;

  const fieldChange = onFieldChange ?? onFielChange;
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
          type={type}
          placeholder={placeholder}
          disabled={!!readOnly}
          aria-invalid={!!errorsMessage}
          className={styles.input}
          onChange={(e) => {
            void regOnChange(e);
            propOnChange?.(e);
            fieldChange?.(e);
          }}
        />
        {EndIcon ? (
          <button type="button" className={styles.iconBtn} onClick={onEndIconClick} tabIndex={-1} aria-label="Ação">
            <EndIcon width={18} height={18} />
          </button>
        ) : null}
      </div>

      {errorsMessage ? (
        <span className={styles.error} role="alert">
          {errorsMessage}
        </span>
      ) : hint ? (
        <span className={styles.hint}>{hint}</span>
      ) : null}
    </div>
  );
};
