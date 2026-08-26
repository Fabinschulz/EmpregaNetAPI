'use client';

import { maskCpf } from '@/shared/utils';
import { InputField, type InputFieldProps } from '../input';

export type CpfFieldProps = Omit<InputFieldProps, 'type' | 'inputMode' | 'format'>;

export function CpfField({ placeholder = '000.000.000-00', ...props }: CpfFieldProps) {
  return <InputField inputMode="numeric" maxLength={14} placeholder={placeholder} format={maskCpf} {...props} />;
}
