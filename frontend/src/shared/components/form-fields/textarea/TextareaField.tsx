'use client';

import { Label } from '@/components';
import { useFormContext } from '@/context';
import type React from 'react';
import { cn, getFieldErrorMessage } from '@/utils';
import styles from './textarea.module.scss';

export type TextareaFieldProps = Omit<React.ComponentProps<'textarea'>, 'className'> & {
  name: string;
  label?: string;
  className?: string;
  error?: string;
  hint?: string | null;
  onFieldChange?: (event: React.ChangeEvent<HTMLTextAreaElement>) => void;
};

export const TextareaField: React.FC<TextareaFieldProps> = ({
  name,
  label,
  required,
  placeholder,
  rows = 4,
  className,
  error: errorProp,
  hint,
  onFieldChange,
  ...props
}) => {
  const { register, validationErrors, readOnly } = useFormContext();
  const fromForm = getFieldErrorMessage(name, validationErrors);
  const errorsMessage = errorProp ?? fromForm;
  const labelText = required && label ? `${label} *` : label;

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
      <textarea
        {...restProps}
        {...regRest}
        id={name}
        ref={ref}
        rows={rows}
        placeholder={placeholder}
        disabled={!!readOnly}
        aria-invalid={!!errorsMessage}
        aria-describedby={describedBy}
        className={styles.textarea}
        required={required}
        onChange={(e) => {
          void regOnChange(e);
          propOnChange?.(e);
          onFieldChange?.(e);
        }}
      />
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
