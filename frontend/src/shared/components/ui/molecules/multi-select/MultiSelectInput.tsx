'use client';

import { cn, truncateText } from '@/utils';
import { ChevronDown, X } from 'lucide-react';
import * as React from 'react';
import type { ControlTriggerProps } from '../../control-props';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '../command';
import { Popover, PopoverContent, PopoverTrigger } from '../popover';
import { TooltipProvider } from '../tooltip';
import { TruncatedLabel } from '../truncated-label';
import styles from './MultiSelect.module.scss';

export interface MultiSelectOption {
  label: string;
  value: string;
}

export type MultiSelectInputProps = {
  value: readonly string[];
  onChange: (value: string[]) => void;
  options: readonly MultiSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  /** Opções ainda a caminho: bloqueia o trigger e avisa em vez de mostrar uma lista vazia. */
  loading?: boolean;
  disabled?: boolean;
  className?: string;
  onBlur?: () => void;
  ref?: React.Ref<HTMLDivElement>;
  triggerProps?: ControlTriggerProps;
};

/** Escolha múltipla com chips removíveis e busca dentro da lista. */
export function MultiSelectInput({
  value,
  onChange,
  options,
  placeholder = 'Selecione',
  searchPlaceholder = 'Pesquisar opção...',
  loading = false,
  disabled,
  className,
  onBlur,
  ref,
  triggerProps
}: MultiSelectInputProps) {
  const [open, setOpen] = React.useState(false);
  const [query, setQuery] = React.useState('');
  const listboxId = `${React.useId()}-listbox`;

  const isBlocked = disabled === true || loading;

  /** Um valor sem opção correspondente ainda assim aparece: melhor mostrar a chave que perdê-la. */
  const selectedOptions = value.map(
    (item) => options.find((option) => option.value === item) ?? { value: item, label: item }
  );

  const toggle = (option: string) =>
    onChange(value.includes(option) ? value.filter((item) => item !== option) : [...value, option]);

  const handleTriggerKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (isBlocked || (event.key !== 'Enter' && event.key !== ' ')) {
      return;
    }

    event.preventDefault();
    setOpen((isOpen) => !isOpen);
  };

  const needle = query.toLowerCase();
  const filteredOptions = options.filter(
    (option) => option.label.toLowerCase().includes(needle) || option.value.toLowerCase().includes(needle)
  );

  return (
    <TooltipProvider delayDuration={200}>
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <div
            ref={ref}
            role="combobox"
            onBlur={onBlur}
            tabIndex={isBlocked ? -1 : 0}
            aria-disabled={isBlocked ? true : undefined}
            aria-expanded={open}
            aria-haspopup="listbox"
            aria-controls={listboxId}
            className={cn(styles.trigger, className)}
            onKeyDown={handleTriggerKeyDown}
            {...triggerProps}
          >
            {selectedOptions.length > 0 ? (
              <div className={styles.badges}>
                {selectedOptions.map((option) => (
                  <span key={option.value} className={styles.chip}>
                    {truncateText(option.label, 50)}
                    {!isBlocked ? (
                      <button
                        type="button"
                        className={styles.removeBtn}
                        onClick={(event) => {
                          event.stopPropagation();
                          toggle(option.value);
                        }}
                        aria-label={`Remover ${option.label}`}
                      >
                        <X size={12} />
                      </button>
                    ) : null}
                  </span>
                ))}
              </div>
            ) : (
              <span className={styles.placeholder}>{loading ? 'Carregando...' : placeholder}</span>
            )}
            <ChevronDown className={styles.chevron} size={16} aria-hidden />
          </div>
        </PopoverTrigger>

        <PopoverContent id={listboxId} align="start" className={styles.popoverContent}>
          <Command shouldFilter={false}>
            <CommandInput placeholder={searchPlaceholder} value={query} onValueChange={setQuery} />
            <CommandList>
              <CommandEmpty>Nenhuma opção encontrada.</CommandEmpty>
              <CommandGroup className={styles.commandGroup}>
                {filteredOptions.map((option) => (
                  <CommandItem
                    key={option.value}
                    value={`${option.label} ${option.value}`}
                    onSelect={() => toggle(option.value)}
                  >
                    <span
                      className={cn(styles.checkCell, value.includes(option.value) && styles.checkCellOn)}
                      aria-hidden
                    >
                      {value.includes(option.value) ? <span className={styles.checkDot} /> : null}
                    </span>
                    <TruncatedLabel text={option.label} />
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    </TooltipProvider>
  );
}
