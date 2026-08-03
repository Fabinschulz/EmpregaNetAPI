'use client';

import type { ReactNode } from 'react';
import styles from './choice-group.module.scss';

export type ChoiceOption<T extends string = string> = {
  readonly value: T;
  readonly label: string;
};

/** Grupo nomeado de opções, na ordem em que a UI deve exibi-las. */
export type ChoiceOptionGroup = {
  readonly label: string;
  readonly items: readonly string[];
};

type FieldsetProps = {
  legend: string;
  children: ReactNode;
};

function Fieldset({ legend, children }: FieldsetProps) {
  return (
    <fieldset className={styles.group}>
      <legend className={styles.legend}>{legend}</legend>
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
      <span>{label}</span>
    </label>
  );
}

export type CheckboxGroupProps<T extends string> = {
  legend: string;
  options: readonly ChoiceOption<T>[];
  selected: readonly T[];
  onToggle: (value: T) => void;
  /** Acima deste número de opções a lista passa a rolar em vez de esticar o container. */
  scrollAfter?: number;
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
  scrollAfter
}: CheckboxGroupProps<T>) {
  const isScrollable = scrollAfter !== undefined && options.length > scrollAfter;

  return (
    <Fieldset legend={legend}>
      <div className={isScrollable ? styles.scroll : undefined}>
        {options.map((option) => (
          <OptionRow
            key={option.value}
            type="checkbox"
            label={option.label}
            checked={selected.includes(option.value)}
            onChange={() => onToggle(option.value)}
          />
        ))}
      </div>
    </Fieldset>
  );
}

export type RadioGroupProps = {
  legend: string;
  /** Nome do grupo no DOM; precisa ser único na página. */
  name: string;
  options: readonly ChoiceOption[];
  selected: string | null;
  onSelect: (value: string | null) => void;
};

/**
 * Escolha única, com desmarcação: clicar na opção já selecionada limpa o valor (`null`).
 *
 * Um radio nativo não desmarca; sem isto o utilizador que escolhe uma faixa salarial por
 * engano fica preso a ela até recarregar a página.
 */
export function RadioGroup({ legend, name, options, selected, onSelect }: RadioGroupProps) {
  return (
    <Fieldset legend={legend}>
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
    </Fieldset>
  );
}

export type GroupedCheckboxesProps = {
  legend: string;
  groups: readonly ChoiceOptionGroup[];
  selected: readonly string[];
  onToggle: (value: string) => void;
};

/**
 * Seleção múltipla em subgrupos nomeados. O agrupamento é o que torna navegável uma lista de
 * dezenas de itens (requisitos, benefícios) que corrida ninguém percorre.
 */
export function GroupedCheckboxes({ legend, groups, selected, onToggle }: GroupedCheckboxesProps) {
  if (groups.length === 0) return null;

  return (
    <Fieldset legend={legend}>
      <div className={styles.scroll}>
        {groups.map((group) => (
          <div key={group.label} className={styles.subGroup}>
            <p className={styles.subGroupLabel}>{group.label}</p>
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
        ))}
      </div>
    </Fieldset>
  );
}
