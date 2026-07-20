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
import { useDebouncedValue } from '@/hooks';
import { cn, getFieldErrorMessage } from '@/utils';
import { Loader2, Search } from 'lucide-react';
import * as React from 'react';
import styles from './autocomplete.module.scss';

export interface AutocompleteOption {
  label: string;
  value: string;
}

export interface AutocompleteFieldProps {
  name: string;
  label?: string;
  placeholder?: string;
  options: AutocompleteOption[];
  loading?: boolean;
  debounceMs?: number;
  className?: string;
}

export const AutocompleteField: React.FC<AutocompleteFieldProps> = ({
  name,
  label,
  placeholder = 'Buscar...',
  options,
  loading = false,
  debounceMs = 350,
  className
}) => {
  const { setValue, watch, validationErrors, readOnly } = useFormContext();
  const committed = ((watch(name) as string | undefined) ?? '').toString();

  const [open, setOpen] = React.useState(false);
  const [input, setInput] = React.useState(committed);
  const debouncedInput = useDebouncedValue(input, debounceMs);
  const lastCommittedRef = React.useRef(committed);

  const error = getFieldErrorMessage(name, validationErrors);
  const labelId = `${name}-label`;
  const errorId = `${name}-error`;

  /** Escreve o termo no formulário -> muda o param do useQuery -> busca no servidor. */
  const commit = React.useCallback(
    (raw: string) => {
      const value = raw.trim();
      lastCommittedRef.current = value;
      setValue(name, value, { shouldDirty: true });
    },
    [name, setValue]
  );

  React.useEffect(() => {
    if (debouncedInput.trim() !== lastCommittedRef.current) {
      commit(debouncedInput);
    }
  }, [debouncedInput, commit]);

  React.useEffect(() => {
    if (committed !== lastCommittedRef.current) {
      lastCommittedRef.current = committed;
      setInput(committed);
    }
  }, [committed]);

  const handleSelect = (option: AutocompleteOption) => {
    setInput(option.label);
    commit(option.label);
    setOpen(false);
  };

  return (
    <div className={cn(styles.field, className)}>
      {label ? (
        <span id={labelId} className={styles.label}>
          {label}
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
            aria-labelledby={label ? labelId : undefined}
            aria-describedby={error ? errorId : undefined}
            data-invalid={error ? 'true' : undefined}
          >
            <Search className={styles.icon} size={16} aria-hidden />
            <span className={cn(styles.triggerText, !committed && styles.triggerMuted)}>
              {committed || placeholder}
            </span>
          </button>
        </PopoverTrigger>
        <PopoverContent align="start" className={styles.popoverContent}>
          <Command shouldFilter={false}>
            <CommandInput placeholder="Digite para buscar..." value={input} onValueChange={setInput} />
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
                  <CommandItem key={option.value} value={option.value} onSelect={() => handleSelect(option)}>
                    {option.label}
                  </CommandItem>
                ))
              )}
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
