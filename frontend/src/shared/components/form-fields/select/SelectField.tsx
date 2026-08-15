'use client';

import { SelectInput, type SelectOption } from '@/components/ui';
import type React from 'react';
import { FormField, useFormField, type FormFieldBaseProps } from '../field';

export type SelectFieldProps = FormFieldBaseProps & {
  options: readonly SelectOption[];
  placeholder?: string;
  /** Ver {@link import('@/components/ui').SelectInputProps.loading}. */
  loading?: boolean;
};

/** Escolha única ligada ao formulário. Toda a apresentação vem do `SelectInput` da UI. */
export const SelectField: React.FC<SelectFieldProps> = ({
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
  const { control, ref, value, setValue, error, ids } = useFormField<string | undefined>({
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
      <SelectInput
        value={value}
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
