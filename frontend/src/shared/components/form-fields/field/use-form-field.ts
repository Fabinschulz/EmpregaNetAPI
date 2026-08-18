'use client';

import { useFormContext } from '@/context';
import { getFieldErrorMessage } from '@/utils';
import { useId } from 'react';
import { useController, type RefCallBack } from 'react-hook-form';

export type FormFieldIds = {
  control: string;
  label: string;
  error: string;
  hint: string;
};

export type FormFieldBaseProps = {
  name: string;
  label?: string;
  required?: boolean;
  hint?: string | null;
  error?: string;
  disabled?: boolean;
  className?: string;
};

export type FormFieldControl = {
  name: string;
  disabled: boolean;
  invalid: boolean;
  onBlur: () => void;
  'aria-describedby': string | undefined;
};

export type UseFormFieldResult<TValue> = {
  control: FormFieldControl;
  /** Liga o controle ao foco automático do formulário no primeiro campo inválido. */
  ref: RefCallBack;
  value: TValue;
  /** Escreve no formulário marcando `dirty` e revalidando - o erro some assim que o usuário corrige. */
  setValue: (value: TValue) => void;
  error: string | undefined;
  ids: FormFieldIds;
  isDirty: boolean;
  isTouched: boolean;
};

export function useFormField<TValue = unknown>({
  name,
  error: errorProp,
  disabled,
  hint
}: Pick<FormFieldBaseProps, 'name' | 'error' | 'disabled' | 'hint'>): UseFormFieldResult<TValue> {
  const { control: formControl, validationErrors, readOnly } = useFormContext();
  const uid = useId();

  const { field, fieldState } = useController({ name, control: formControl });

  const error = errorProp ?? getFieldErrorMessage(name, validationErrors);

  const ids: FormFieldIds = {
    control: `${uid}-${name}-control`,
    label: `${uid}-${name}-label`,
    error: `${uid}-${name}-error`,
    hint: `${uid}-${name}-hint`
  };

  return {
    control: {
      name: field.name,
      disabled: disabled === true || readOnly === true,
      invalid: !!error,
      onBlur: field.onBlur,
      'aria-describedby': error ? ids.error : hint ? ids.hint : undefined
    },
    ref: field.ref,
    value: field.value as TValue,
    setValue: (value: TValue) => field.onChange(value),
    error,
    ids,
    isDirty: fieldState.isDirty,
    isTouched: fieldState.isTouched
  };
}
