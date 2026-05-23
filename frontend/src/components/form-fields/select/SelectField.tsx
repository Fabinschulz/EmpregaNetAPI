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
} from '@/components';
import { useFormContext } from '@/context';
import type React from 'react';
import { cn, getFieldErrorMessage, truncateText } from 'src/utils';
import styles from './select.module.scss';

export interface SelectOption {
  label: string;
  value: string;
}

export type SelectFieldProps = {
  name: string;
  options: SelectOption[];
  label?: string;
  required?: boolean;
  placeholder?: string;
  className?: string;
};

export const SelectField: React.FC<SelectFieldProps> = ({
  name,
  options,
  required,
  label,
  placeholder = 'Selecione',
  className
}) => {
  const { validationErrors, setValue, watch, readOnly } = useFormContext();
  const errorsMessage = getFieldErrorMessage(name, validationErrors);
  const currentValue = watch(name) as string | undefined;
  const labelText = required && label ? `${label} *` : label;

  const onChange = (value: string) => {
    const found = options.find((o) => o.value === value);
    if (found) {
      setValue(name, found.value, { shouldDirty: true });
    }
  };

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? <span className={styles.label}>{labelText}</span> : null}

      <TooltipProvider delayDuration={200}>
        <Select
          value={currentValue != null && currentValue !== '' ? String(currentValue) : undefined}
          onValueChange={onChange}
          disabled={!!readOnly}
          name={name}
        >
          <SelectTrigger aria-invalid={!!errorsMessage} data-testid={`select-${name}-id`}>
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
          <SelectContent>
            <SelectGroup>
              {options.map((option) => {
                const isLong = option.label.length > 215;
                const displayText = truncateText(option.label, 210);

                return (
                  <SelectItem key={option.value} value={option.value}>
                    {isLong ? (
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <span>{displayText}</span>
                        </TooltipTrigger>
                        <TooltipContent>{option.label}</TooltipContent>
                      </Tooltip>
                    ) : (
                      <span>{option.label}</span>
                    )}
                  </SelectItem>
                );
              })}
            </SelectGroup>
          </SelectContent>
        </Select>
      </TooltipProvider>

      {errorsMessage ? <span className={styles.error}>{errorsMessage}</span> : null}
    </div>
  );
};
