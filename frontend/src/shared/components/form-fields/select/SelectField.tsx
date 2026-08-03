'use client';

import { useFormContext } from '@/context';
import { cn, getFieldErrorMessage } from '@/utils';
import type React from 'react';
import { SelectInput, type SelectOption } from './SelectInput';
import styles from './select.module.scss';

export type { SelectOption };

export type SelectFieldProps = {
  name: string;
  /** `readonly` para aceitar direto as listas `as const` do vocabulário, sem cópia intermediária. */
  options: readonly SelectOption[];
  label?: string;
  required?: boolean;
  placeholder?: string;
  className?: string;
};

/**
 * Select ligado ao `react-hook-form`: rótulo, erro de validação e escrita no formulário.
 * A composição do Radix vive em {@link SelectInput}, partilhada com superfícies sem formulário.
 */
export const SelectField: React.FC<SelectFieldProps> = ({
  name,
  options,
  required,
  label,
  placeholder,
  className
}) => {
  const { validationErrors, setValue, watch, readOnly } = useFormContext();
  const errorMessage = getFieldErrorMessage(name, validationErrors);
  const labelText = required && label ? `${label} *` : label;

  const labelId = `${name}-label`;
  const errorId = `${name}-error`;

  const handleChange = (value: string) => {
    if (options.some((option) => option.value === value)) {
      setValue(name, value, { shouldDirty: true });
    }
  };

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? (
        <span id={labelId} className={styles.label}>
          {labelText}
        </span>
      ) : null}

      <SelectInput
        value={watch(name) as string | undefined}
        onChange={handleChange}
        options={options}
        placeholder={placeholder}
        disabled={!!readOnly}
        triggerProps={{
          name,
          'aria-invalid': !!errorMessage,
          'aria-labelledby': labelText ? labelId : undefined,
          'aria-describedby': errorMessage ? errorId : undefined,
          'data-testid': `select-${name}-id`
        }}
      />

      {errorMessage ? (
        <span id={errorId} className={styles.error} role="alert">
          {errorMessage}
        </span>
      ) : null}
    </div>
  );
};
