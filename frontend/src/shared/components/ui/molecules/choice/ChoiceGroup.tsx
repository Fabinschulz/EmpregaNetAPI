'use client';

import { cn } from '@/utils';
import { Search } from 'lucide-react';
import { useId, useMemo, useState, type ReactNode } from 'react';
import styles from './ChoiceGroup.module.scss';

export type ChoiceOption<T extends string = string> = {
  readonly value: T;
  readonly label: string;
};

/** Grupo nomeado de opções, na ordem em que a UI deve exibi-las. */
export type ChoiceOptionGroup = {
  readonly label: string;
  readonly items: readonly string[];
};

function normalize(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase();
}

type FieldsetProps = {
  legend: string;
  legendHidden?: boolean;
  children: ReactNode;
};

function Fieldset({ legend, legendHidden, children }: FieldsetProps) {
  return (
    <fieldset className={styles.group}>
      <legend className={legendHidden ? 'sr-only' : styles.legend}>{legend}</legend>
      {children}
    </fieldset>
  );
}

type OptionRowProps = {
  type: 'checkbox' | 'radio';
  /** Obrigatório para `radio`: agrupa os botões que se excluem entre si. */
  name?: string;
  label: string;
  checked: boolean;
  onChange: () => void;
  /** Chamado no clique mesmo quando já selecionado - permite desmarcar um radio. */
  onClick?: () => void;
};

function OptionRow({ type, name, label, checked, onChange, onClick }: OptionRowProps) {
  return (
    <label className={styles.option}>
      <input
        type={type}
        name={name}
        className={styles.control}
        checked={checked}
        onChange={onChange}
        onClick={onClick}
      />
      <span className={styles.optionLabel}>{label}</span>
    </label>
  );
}

type ChoiceSearchProps = {
  legend: string;
  value: string;
  onChange: (value: string) => void;
};

function ChoiceSearch({ legend, value, onChange }: ChoiceSearchProps) {
  const inputId = useId();

  return (
    <div className={styles.search}>
      <label className="sr-only" htmlFor={inputId}>
        Filtrar opções de {legend.toLowerCase()}
      </label>

      <Search className={styles.searchIcon} aria-hidden />

      <input
        id={inputId}
        type="search"
        className={styles.searchInput}
        placeholder="Filtrar opções..."
        value={value}
        autoComplete="off"
        onChange={(event) => onChange(event.target.value)}
      />
    </div>
  );
}

function NoMatches({ query }: { query: string }) {
  return (
    <p className={styles.empty} role="status">
      Nenhuma opção corresponde a &ldquo;{query}&rdquo;.
    </p>
  );
}

/** Regras de apresentação partilhadas por todos os grupos. */
type ChoiceLayoutProps = {
  /** Distribui as opções numa grelha responsiva em vez de uma coluna única. */
  columns?: boolean;
  /** Acima deste número de opções a lista passa a rolar em vez de esticar o container. */
  scrollAfter?: number;
  /** Acima deste número de opções aparece um campo para filtrar a própria lista. */
  searchAfter?: number;
};

export type CheckboxGroupProps<T extends string> = ChoiceLayoutProps & {
  legend: string;
  options: readonly ChoiceOption<T>[];
  selected: readonly T[];
  onToggle: (value: T) => void;
  /** Ver {@link FieldsetProps.legendHidden}. */
  legendHidden?: boolean;
};

/**
 * Seleção múltipla por checkbox. Nenhuma opção marcada significa "todas" para quem consome:
 * o componente só reporta o toggle, a interpretação fica com o filtro.
 */
export function CheckboxGroup<T extends string>({
  legend,
  options,
  selected,
  onToggle,
  scrollAfter,
  searchAfter,
  columns,
  legendHidden
}: CheckboxGroupProps<T>) {
  const [query, setQuery] = useState('');

  const isSearchable = searchAfter !== undefined && options.length > searchAfter;
  const isScrollable = scrollAfter !== undefined && options.length > scrollAfter;

  const visible = useMemo(() => {
    if (!isSearchable || !query.trim()) return options;

    const needle = normalize(query.trim());
    return options.filter((option) => normalize(option.label).includes(needle));
  }, [isSearchable, options, query]);

  return (
    <Fieldset legend={legend} legendHidden={legendHidden}>
      {isSearchable ? <ChoiceSearch legend={legend} value={query} onChange={setQuery} /> : null}

      {visible.length === 0 ? (
        <NoMatches query={query.trim()} />
      ) : (
        <div className={cn(isScrollable && styles.scroll, columns && styles.columns)}>
          {visible.map((option) => (
            <OptionRow
              key={option.value}
              type="checkbox"
              label={option.label}
              checked={selected.includes(option.value)}
              onChange={() => onToggle(option.value)}
            />
          ))}
        </div>
      )}
    </Fieldset>
  );
}

export type RadioGroupProps = Pick<ChoiceLayoutProps, 'columns'> & {
  legend: string;
  /** Nome do grupo no DOM; precisa ser único na página. */
  name: string;
  options: readonly ChoiceOption[];
  selected: string | null;
  onSelect: (value: string | null) => void;
  /** Ver {@link FieldsetProps.legendHidden}. */
  legendHidden?: boolean;
};

/**
 * Escolha única, com desmarcação: clicar na opção já selecionada limpa o valor (`null`).
 *
 * Um radio nativo não desmarca; sem isto o utilizador que escolhe uma faixa salarial por
 * engano fica preso a ela até recarregar a página.
 */
export function RadioGroup({ legend, name, options, selected, onSelect, columns, legendHidden }: RadioGroupProps) {
  return (
    <Fieldset legend={legend} legendHidden={legendHidden}>
      <div className={cn(columns && styles.columns)}>
        {options.map((option) => (
          <OptionRow
            key={option.value}
            type="radio"
            name={name}
            label={option.label}
            checked={selected === option.value}
            onChange={() => onSelect(option.value)}
            onClick={() => selected === option.value && onSelect(null)}
          />
        ))}
      </div>
    </Fieldset>
  );
}

export type GroupedCheckboxesProps = ChoiceLayoutProps & {
  legend: string;
  groups: readonly ChoiceOptionGroup[];
  selected: readonly string[];
  onToggle: (value: string) => void;
  /** Ver {@link FieldsetProps.legendHidden}. */
  legendHidden?: boolean;
};

/**
 * Seleção múltipla em subgrupos nomeados. O agrupamento é o que torna navegável uma lista de
 * dezenas de itens (requisitos, benefícios) que corrida ninguém percorre.
 */
export function GroupedCheckboxes({
  legend,
  groups,
  selected,
  onToggle,
  scrollAfter,
  searchAfter,
  columns,
  legendHidden
}: GroupedCheckboxesProps) {
  const [query, setQuery] = useState('');

  const total = useMemo(() => groups.reduce((sum, group) => sum + group.items.length, 0), [groups]);

  const isSearchable = searchAfter !== undefined && total > searchAfter;
  const isScrollable = scrollAfter !== undefined && total > scrollAfter;

  /** Subgrupos que ficam sem itens após a busca somem - um título órfão só faz ruído. */
  const visible = useMemo(() => {
    if (!isSearchable || !query.trim()) return groups;

    const needle = normalize(query.trim());
    return groups
      .map((group) => ({ ...group, items: group.items.filter((item) => normalize(item).includes(needle)) }))
      .filter((group) => group.items.length > 0);
  }, [groups, isSearchable, query]);

  if (groups.length === 0) return null;

  return (
    <Fieldset legend={legend} legendHidden={legendHidden}>
      {isSearchable ? <ChoiceSearch legend={legend} value={query} onChange={setQuery} /> : null}

      {visible.length === 0 ? (
        <NoMatches query={query.trim()} />
      ) : (
        <div className={cn(isScrollable && styles.scroll)}>
          {visible.map((group) => (
            <div key={group.label} className={styles.subGroup}>
              <p className={styles.subGroupLabel}>{group.label}</p>

              <div className={cn(columns && styles.columns)}>
                {group.items.map((item) => (
                  <OptionRow
                    key={item}
                    type="checkbox"
                    label={item}
                    checked={selected.includes(item)}
                    onChange={() => onToggle(item)}
                  />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </Fieldset>
  );
}
