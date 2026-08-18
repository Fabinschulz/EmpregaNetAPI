'use client';

import { AutocompleteInput, type AutocompleteOption } from '@/shared/components/ui';
import type React from 'react';
import { FormField, useFormField, type FormFieldBaseProps } from '../field';

export type AutocompleteFieldProps = FormFieldBaseProps & {
  options: readonly AutocompleteOption[];
  placeholder?: string;
  loading?: boolean;
  debounceMs?: number;
};

export const AutocompleteField: React.FC<AutocompleteFieldProps> = ({
  name,
  label,
  required,
  hint,
  placeholder,
  options,
  loading,
  debounceMs,
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
      <AutocompleteInput
        value={value ?? ''}
        onChange={setValue}
        options={options}
        placeholder={placeholder}
        loading={loading}
        debounceMs={debounceMs}
        disabled={control.disabled}
        onBlur={control.onBlur}
        ref={ref}
        triggerProps={{
          id: ids.control,
          name: control.name,
          'aria-invalid': control.invalid,
          'aria-labelledby': label ? ids.label : undefined,
          'aria-describedby': control['aria-describedby'],
          'data-testid': `autocomplete-${name}-id`
        }}
      />
    </FormField>
  );
};
