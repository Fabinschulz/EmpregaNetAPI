'use client';

import type * as React from 'react';
import type { ControlTriggerProps } from '../../control-props';
import { TooltipProvider } from '../tooltip';
import { TruncatedLabel } from '../truncated-label';
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from './Select';

export interface SelectOption {
  label: string;
  value: string;
  disabled?: boolean;
}

export type SelectInputProps = {
  value: string | null | undefined;
  onChange: (value: string) => void;
  options: readonly SelectOption[];
  placeholder?: string;
  disabled?: boolean;
  loading?: boolean;
  className?: string;
  onBlur?: () => void;
  ref?: React.Ref<HTMLButtonElement>;
  triggerProps?: ControlTriggerProps;
};

/**
 * <para>Serve tanto os campos de formulário (`SelectField`) como superfícies sem formulário
 * (ordenação do feed, filtros locais).</para>
 */
export function SelectInput({
  value,
  onChange,
  options,
  placeholder = 'Selecione',
  disabled,
  loading = false,
  className,
  onBlur,
  ref,
  triggerProps
}: SelectInputProps) {
  return (
    <TooltipProvider delayDuration={200}>
      <Select
        value={value != null && value !== '' ? String(value) : undefined}
        onValueChange={onChange}
        disabled={disabled || loading}
        name={triggerProps?.name}
      >
        <SelectTrigger ref={ref} className={className} onBlur={onBlur} {...triggerProps}>
          <SelectValue placeholder={loading ? 'Carregando...' : placeholder} />
        </SelectTrigger>

        <SelectContent>
          <SelectGroup>
            {options.map((option) => (
              <SelectItem key={option.value} value={option.value} disabled={option.disabled}>
                <TruncatedLabel text={option.label} />
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </TooltipProvider>
  );
}
