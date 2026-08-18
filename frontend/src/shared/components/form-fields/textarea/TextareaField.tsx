'use client';

import { Textarea, type TextareaProps } from '@/shared/components/ui';
import type React from 'react';
import { FormField, useFormField, type FormFieldBaseProps } from '../field';

export type TextareaFieldProps = FormFieldBaseProps &
  Omit<TextareaProps, 'name' | 'value' | 'defaultValue' | 'invalid' | 'required'>;

/** Campo multilinha ligado ao formulário. Toda a apresentação vem do {@link Textarea} da UI. */
export const TextareaField: React.FC<TextareaFieldProps> = ({
  name,
  label,
  required,
  hint,
  error: errorProp,
  disabled,
  className,
  onChange,
  ...textareaProps
}) => {
  const { control, ref, value, setValue, error, ids } = useFormField<string | undefined>({
    name,
    error: errorProp,
    disabled,
    hint
  });

  return (
    <FormField ids={ids} label={label} required={required} error={error} hint={hint} className={className}>
      <Textarea
        {...textareaProps}
        {...control}
        ref={ref}
        id={ids.control}
        value={value ?? ''}
        onChange={(event) => {
          setValue(event.target.value);
          onChange?.(event);
        }}
      />
    </FormField>
  );
};
