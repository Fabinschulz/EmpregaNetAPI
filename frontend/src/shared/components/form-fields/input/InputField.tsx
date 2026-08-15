'use client';

import { Input, type InputProps } from '@/components/ui';
import type React from 'react';
import { FormField, useFormField, type FormFieldBaseProps } from '../field';

export type InputFieldProps = FormFieldBaseProps &
  Omit<InputProps, 'name' | 'value' | 'defaultValue' | 'invalid' | 'required'> & {
    format?: (value: string) => string;
  };

/** Campo de texto ligado ao formulário. Toda a apresentação vem do {@link Input} da UI. */
export const InputField: React.FC<InputFieldProps> = ({
  name,
  label,
  required,
  hint,
  error: errorProp,
  disabled,
  className,
  format,
  onChange,
  ...inputProps
}) => {
  const { control, ref, value, setValue, error, ids } = useFormField<string | undefined>({
    name,
    error: errorProp,
    disabled,
    hint
  });

  return (
    <FormField ids={ids} label={label} required={required} error={error} hint={hint} className={className}>
      <Input
        {...inputProps}
        {...control}
        ref={ref}
        id={ids.control}
        value={value ?? ''}
        onChange={(event) => {
          setValue(format ? format(event.target.value) : event.target.value);
          onChange?.(event);
        }}
      />
    </FormField>
  );
};
