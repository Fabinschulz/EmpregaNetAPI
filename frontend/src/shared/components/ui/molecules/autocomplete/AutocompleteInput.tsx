'use client';

import { useDebouncedDraft } from '@/shared/hooks';
import { cn } from '@/shared/utils';
import { Loader2, Search } from 'lucide-react';
import * as React from 'react';
import type { ControlTriggerProps } from '../../control-props';
import { Command, CommandEmpty, CommandInput, CommandItem, CommandList } from '../command';
import { Popover, PopoverContent, PopoverTrigger } from '../popover';
import styles from './Autocomplete.module.scss';

export interface AutocompleteOption {
  label: string;
  value: string;
}

export type AutocompleteInputProps = {
  value: string;
  /** Recebe o texto confirmado: pelo debounce enquanto se digita, ou imediato ao escolher uma opção. */
  onChange: (value: string) => void;
  options: readonly AutocompleteOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  /** Consulta das sugestões em curso - mostra o indicador em vez de "nenhum resultado". */
  loading?: boolean;
  debounceMs?: number;
  disabled?: boolean;
  className?: string;
  onBlur?: () => void;
  ref?: React.Ref<HTMLButtonElement>;
  triggerProps?: ControlTriggerProps;
};

export function AutocompleteInput({
  value,
  onChange,
  options,
  placeholder = 'Buscar...',
  searchPlaceholder = 'Digite para buscar...',
  loading = false,
  debounceMs = 350,
  disabled,
  className,
  onBlur,
  ref,
  triggerProps
}: AutocompleteInputProps) {
  const [open, setOpen] = React.useState(false);

  const { draft, setDraft, commitNow } = useDebouncedDraft({ value, onCommit: onChange, delayMs: debounceMs });

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <button
          type="button"
          ref={ref}
          disabled={disabled}
          onBlur={onBlur}
          className={cn(styles.trigger, className)}
          aria-expanded={open}
          aria-haspopup="listbox"
          {...triggerProps}
        >
          <Search className={styles.icon} size={16} aria-hidden />
          <span className={cn(styles.triggerText, !value && styles.triggerMuted)}>{value || placeholder}</span>
        </button>
      </PopoverTrigger>

      <PopoverContent align="start" className={styles.popoverContent}>
        <Command shouldFilter={false}>
          <CommandInput placeholder={searchPlaceholder} value={draft} onValueChange={setDraft} />
          <CommandList>
            {loading ? (
              <div className={styles.status} role="status">
                <Loader2 className={styles.spinner} size={16} aria-hidden />
                Buscando...
              </div>
            ) : options.length === 0 ? (
              <CommandEmpty>Nenhum resultado encontrado.</CommandEmpty>
            ) : (
              options.map((option) => (
                <CommandItem
                  key={option.value}
                  value={option.value}
                  onSelect={() => {
                    commitNow(option.label);
                    setOpen(false);
                  }}
                >
                  {option.label}
                </CommandItem>
              ))
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
