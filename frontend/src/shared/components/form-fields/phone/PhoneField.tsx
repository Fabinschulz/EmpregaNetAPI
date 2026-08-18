'use client';

import { maskBrazilPhone } from '@/shared/utils';
import { InputField, type InputFieldProps } from '../input';

export type PhoneFieldProps = Omit<InputFieldProps, 'type' | 'inputMode' | 'format'>;

/**
 * Campo de telefone com máscara brasileira aplicada enquanto o usuário digita
 * (fixo `(00) 0000-0000` ou celular `(00) 00000-0000`). A validação do formato
 * fica a cargo do schema do formulário (ex.: `isValidBrazilPhone`).
 */
export function PhoneField({ placeholder = '(00) 99999-9999', ...props }: PhoneFieldProps) {
  return <InputField inputMode="tel" maxLength={15} placeholder={placeholder} format={maskBrazilPhone} {...props} />;
}
