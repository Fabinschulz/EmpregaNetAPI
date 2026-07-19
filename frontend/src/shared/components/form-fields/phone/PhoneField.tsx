'use client';

import { useFormContext } from '@/context';
import { maskBrazilPhone } from '@/utils';
import type { ChangeEvent } from 'react';
import { InputField, type InputFieldProps } from '../input';

export type PhoneFieldProps = Omit<InputFieldProps, 'type' | 'inputMode' | 'onFieldChange'>;

/**
 * Campo de telefone com máscara brasileira aplicada enquanto o usuário digita
 * (fixo `(00) 0000-0000` ou celular `(00) 00000-0000`). A validação do formato
 * fica a cargo do schema do formulário (ex.: `isValidBrazilPhone`).
 */
export function PhoneField({ name, placeholder = '(00) 99999-9999', ...props }: PhoneFieldProps) {
  const { setValue } = useFormContext();

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    setValue(name, maskBrazilPhone(event.target.value), { shouldDirty: true, shouldValidate: true });
  };

  return (
    <InputField
      name={name}
      inputMode="tel"
      maxLength={15}
      placeholder={placeholder}
      onFieldChange={handleChange}
      {...props}
    />
  );
}
