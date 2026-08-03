'use client';

import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger
} from '@/components/ui';
import { truncateText } from '@/utils';
import type { ReactNode } from 'react';

export interface SelectOption {
  label: string;
  value: string;
  disabled?: boolean;
}

const MAX_LABEL_LENGTH = 210;
const TOOLTIP_THRESHOLD = 215;

export type SelectInputProps = {
  value: string | null | undefined;
  onChange: (value: string) => void;
  options: readonly SelectOption[];
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  /** Repassados ao trigger; o wrapper (`SelectField`, `FeedSortSelect`) liga rótulo e erro. */
  triggerProps?: {
    id?: string;
    name?: string;
    'aria-label'?: string;
    'aria-labelledby'?: string;
    'aria-describedby'?: string;
    'aria-invalid'?: boolean;
    'data-testid'?: string;
  };
};

/**
 * Select controlado, sem formulário: recebe `value`/`onChange` e nada mais.
 * Existe para que superfícies fora de `react-hook-form`.
 */
export function SelectInput({
  value,
  onChange,
  options,
  placeholder = 'Selecione',
  disabled,
  className,
  triggerProps
}: SelectInputProps) {
  return (
    <TooltipProvider delayDuration={200}>
      <Select
        value={value != null && value !== '' ? String(value) : undefined}
        onValueChange={onChange}
        disabled={disabled}
        name={triggerProps?.name}
      >
        <SelectTrigger className={className} {...triggerProps}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>

        <SelectContent>
          <SelectGroup>
            {options.map((option) => (
              <SelectItem key={option.value} value={option.value} disabled={option.disabled}>
                {renderLabel(option.label)}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </TooltipProvider>
  );
}

function renderLabel(label: string): ReactNode {
  if (label.length <= TOOLTIP_THRESHOLD) {
    return <span>{label}</span>;
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span>{truncateText(label, MAX_LABEL_LENGTH)}</span>
      </TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  );
}
