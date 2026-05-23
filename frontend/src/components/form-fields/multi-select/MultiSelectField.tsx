'use client';

import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  Popover,
  PopoverContent,
  PopoverTrigger,
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger
} from '@/components';
import { useFormContext } from '@/context';
import { ChevronDown, X } from 'lucide-react';
import * as React from 'react';
import { cn, getFieldErrorMessage, truncateText } from 'src/utils';
import styles from './multi-select.module.scss';

export interface MultiSelectOption {
  label: string;
  value: string;
}

export type MultiSelectFieldProps = {
  name: string;
  options: MultiSelectOption[];
  label?: string;
  required?: boolean;
  placeholder?: string;
  className?: string;
};

export const MultiSelectField: React.FC<MultiSelectFieldProps> = ({
  name,
  options,
  required,
  label,
  placeholder = 'Selecione',
  className
}) => {
  const { validationErrors, setValue, watch, readOnly } = useFormContext();
  const [open, setOpen] = React.useState(false);
  const [query, setQuery] = React.useState('');

  const errorsMessage = getFieldErrorMessage(name, validationErrors);
  const raw = watch(name);
  const currentValues = raw ?? [];
  const selectedValues: string[] = Array.isArray(currentValues)
    ? (currentValues as string[])
    : currentValues
      ? [String(currentValues)]
      : [];

  const selectedOptions = options.filter((option) => selectedValues.includes(option.value));
  const labelText = required && label ? `${label} *` : label;

  const handleSelect = (value: string) => {
    const isSelected = selectedValues.includes(value);
    if (isSelected) {
      setValue(
        name,
        selectedValues.filter((v) => v !== value),
        { shouldDirty: true }
      );
    } else {
      setValue(name, [...selectedValues, value], { shouldDirty: true });
    }
  };

  const handleRemove = (value: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setValue(
      name,
      selectedValues.filter((v) => v !== value),
      { shouldDirty: true }
    );
  };

  const filteredOptions = options.filter(
    (o) => o.label.toLowerCase().includes(query.toLowerCase()) || o.value.toLowerCase().includes(query.toLowerCase())
  );

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? <span className={styles.label}>{labelText}</span> : null}

      <TooltipProvider delayDuration={200}>
        <Popover open={open} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <button
              type="button"
              disabled={!!readOnly}
              data-testid={`select-${name}-id`}
              className={styles.trigger}
              aria-expanded={open}
              aria-haspopup="listbox"
            >
              {selectedValues.length > 0 ? (
                <div className={styles.badges}>
                  {selectedOptions.map((option) => (
                    <span key={option.value} className={styles.chip}>
                      {truncateText(option.label, 50)}
                      {!readOnly ? (
                        <button
                          type="button"
                          className={styles.removeBtn}
                          onClick={(e) => handleRemove(option.value, e)}
                          aria-label={`Remover ${option.label}`}
                        >
                          <X size={12} />
                        </button>
                      ) : null}
                    </span>
                  ))}
                </div>
              ) : (
                <span className={styles.placeholder}>{placeholder}</span>
              )}
              <ChevronDown className={styles.chevron} size={16} aria-hidden />
            </button>
          </PopoverTrigger>
          <PopoverContent align="start" className={styles.popoverContent}>
            <Command shouldFilter={false}>
              <CommandInput placeholder="Pesquisar opção..." value={query} onValueChange={setQuery} />
              <CommandList>
                <CommandEmpty>Nenhuma opção encontrada.</CommandEmpty>
                <CommandGroup className={styles.commandGroup}>
                  {filteredOptions.map(({ label: optLabel, value }) => {
                    const isSelected = selectedValues.includes(value);
                    const isLong = optLabel.length > 215;
                    const displayText = truncateText(optLabel, 210);

                    return (
                      <CommandItem key={value} value={`${optLabel} ${value}`} onSelect={() => handleSelect(value)}>
                        <span className={cn(styles.checkCell, isSelected && styles.checkCellOn)} aria-hidden>
                          {isSelected ? <span className={styles.checkDot} /> : null}
                        </span>
                        {isLong ? (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <span>{displayText}</span>
                            </TooltipTrigger>
                            <TooltipContent>{optLabel}</TooltipContent>
                          </Tooltip>
                        ) : (
                          <span>{optLabel}</span>
                        )}
                      </CommandItem>
                    );
                  })}
                </CommandGroup>
              </CommandList>
            </Command>
          </PopoverContent>
        </Popover>
      </TooltipProvider>

      {errorsMessage ? <span className={styles.error}>{errorsMessage}</span> : null}
    </div>
  );
};
