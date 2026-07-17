'use client';

import {
  Command,
  CommandEmpty,
  CommandInput,
  CommandItem,
  CommandList,
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui';
import { useFormContext } from '@/context';
import { Check, ChevronDown } from 'lucide-react';
import * as React from 'react';
import { cn, getFieldErrorMessage } from '@/utils';
import styles from './autocomplete.module.scss';

export interface AutocompleteOption {
  label: string;
  value: string;
}

export interface AutocompleteFieldProps {
  name: string;
  options: AutocompleteOption[];
  label?: string;
  placeholder?: string;
  required?: boolean;
  className?: string;
}

export const AutocompleteField: React.FC<AutocompleteFieldProps> = ({
  name,
  label,
  required,
  options,
  placeholder = 'Selecione',
  className
}) => {
  const { setValue, watch, validationErrors, readOnly } = useFormContext();
  const [open, setOpen] = React.useState(false);
  const [input, setInput] = React.useState('');
  const selectedValue = watch(name) as string | undefined;
  const selectedOption = options.find((opt) => opt.value === selectedValue);
  const error = getFieldErrorMessage(name, validationErrors);
  const labelText = required && label ? `${label} *` : label;

  const labelId = `${name}-label`;
  const errorId = `${name}-error`;

  const handleSelect = (value: string) => {
    setValue(name, value, { shouldDirty: true });
    setOpen(false);
    setInput('');
  };

  return (
    <div className={cn(styles.field, className)}>
      {labelText ? (
        <span id={labelId} className={styles.label}>
          {labelText}
        </span>
      ) : null}

      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <button
            type="button"
            disabled={!!readOnly}
            className={styles.trigger}
            aria-expanded={open}
            aria-haspopup="listbox"
            aria-labelledby={labelText ? labelId : undefined}
            aria-describedby={error ? errorId : undefined}
            data-invalid={error ? 'true' : undefined}
          >
            <span className={cn(!selectedOption && styles.triggerMuted)}>{selectedOption?.label ?? placeholder}</span>
            <ChevronDown className={styles.chevron} size={16} aria-hidden />
          </button>
        </PopoverTrigger>
        <PopoverContent align="start" className={styles.popoverContent}>
          <Command shouldFilter={false}>
            <CommandInput placeholder="Pesquisar..." value={input} onValueChange={setInput} />
            <CommandList>
              <CommandEmpty>Nenhuma opção encontrada.</CommandEmpty>
              {options
                .filter((opt) => opt.label.toLowerCase().includes(input.toLowerCase()))
                .map((opt) => (
                  <CommandItem
                    key={opt.value}
                    value={opt.value}
                    keywords={[opt.label, opt.value]}
                    onSelect={() => handleSelect(opt.value)}
                    className={cn(selectedValue === opt.value && styles.itemSelected)}
                  >
                    <span className={styles.itemRow}>
                      <span>{opt.label}</span>
                      {selectedValue === opt.value ? <Check size={16} aria-hidden /> : null}
                    </span>
                  </CommandItem>
                ))}
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>

      {error ? (
        <span id={errorId} className={styles.error} role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
};
