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
import { cn, getFieldErrorMessage, truncateText } from '@/utils';
import { ChevronDown, X } from 'lucide-react';
import * as React from 'react';
import styles from './multi-select.module.scss';

export interface MultiSelectOption {
  label: string;
  value: string;
}

export type MultiSelectFieldProps = {
  name: string;
  options: readonly MultiSelectOption[];
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

  const selectedOptions: MultiSelectOption[] = selectedValues.map(
    (value) => options.find((option) => option.value === value) ?? { value, label: value }
  );

  const labelText = required && label ? `${label} *` : label;

  const labelId = `${name}-label`;
  const errorId = `${name}-error`;
  const listboxId = `${React.useId()}-listbox`;

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

  const handleTriggerKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (readOnly || (event.key !== 'Enter' && event.key !== ' ')) {
      return;
    }

    event.preventDefault();
    setOpen((isOpen) => !isOpen);
  };

  const filteredOptions = options.filter(
    (o) => o.label.toLowerCase().includes(query.toLowerCase()) || o.value.toLowerCase().includes(query.toLowerCase())
  );

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? (
        <span id={labelId} className={styles.label}>
          {labelText}
        </span>
      ) : null}

      <TooltipProvider delayDuration={200}>
        <Popover open={open} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <div
              role="combobox"
              tabIndex={readOnly ? -1 : 0}
              aria-disabled={readOnly ? true : undefined}
              data-testid={`select-${name}-id`}
              className={styles.trigger}
              aria-expanded={open}
              aria-haspopup="listbox"
              aria-controls={listboxId}
              aria-labelledby={labelText ? labelId : undefined}
              aria-describedby={errorsMessage ? errorId : undefined}
              data-invalid={errorsMessage ? 'true' : undefined}
              onKeyDown={handleTriggerKeyDown}
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
            </div>
          </PopoverTrigger>
          <PopoverContent id={listboxId} align="start" className={styles.popoverContent}>
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

      {errorsMessage ? (
        <span id={errorId} className={styles.error} role="alert">
          {errorsMessage}
        </span>
      ) : null}
    </div>
  );
};
