'use client';

import { MultiSelectInput, type MultiSelectOption } from '@/shared/components/ui';
import type React from 'react';
import { FormField, useFormField, type FormFieldBaseProps } from '../field';

export type MultiSelectFieldProps = FormFieldBaseProps & {
  options: readonly MultiSelectOption[];
  placeholder?: string;
  /** Ver {@link import('@/shared/components/ui').MultiSelectInputProps.loading}. */
  loading?: boolean;
};

function toStringArray(value: unknown): string[] {
  if (Array.isArray(value)) return value.map(String);
  return value == null || value === '' ? [] : [String(value)];
}

export const MultiSelectField: React.FC<MultiSelectFieldProps> = ({
  name,
  options,
  required,
  label,
  hint,
  placeholder,
  loading,
  error: errorProp,
  disabled,
  className
}) => {
  const { control, ref, value, setValue, error, ids } = useFormField<unknown>({
    name,
    error: errorProp,
    disabled,
    hint
  });

  return (
    <FormField
      ids={ids}
      label={label}
      required={required}
      error={error}
      hint={hint}
      className={className}
      labelFor={false}
    >
      <MultiSelectInput
        value={toStringArray(value)}
        onChange={setValue}
        options={options}
        placeholder={placeholder}
        loading={loading}
        disabled={control.disabled}
        onBlur={control.onBlur}
        ref={ref}
        triggerProps={{
          id: ids.control,
          name: control.name,
          'aria-invalid': control.invalid,
          'aria-labelledby': label ? ids.label : undefined,
          'aria-describedby': control['aria-describedby'],
          'data-testid': `select-${name}-id`
        }}
      />
    </FormField>
  );
};
