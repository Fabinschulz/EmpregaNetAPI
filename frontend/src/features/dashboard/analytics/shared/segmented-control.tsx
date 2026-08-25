'use client';

import { cn } from '@/shared/utils';
import styles from './segmented-control.module.scss';

export type SegmentedControlOption<TValue extends string> = {
  value: TValue;
  label: string;
  srLabel?: string;
};

export type SegmentedControlProps<TValue extends string> = {
  label: string;
  value: TValue;
  options: readonly SegmentedControlOption<TValue>[];
  onChange: (value: TValue) => void;
  className?: string;
};

export function SegmentedControl<TValue extends string>({
  label,
  value,
  options,
  onChange,
  className
}: SegmentedControlProps<TValue>) {
  return (
    <div className={cn(styles.group, className)} role="group" aria-label={label}>
      {options.map((option) => {
        const isActive = option.value === value;

        return (
          <button
            key={option.value}
            type="button"
            className={cn(styles.option, isActive && styles.optionActive)}
            aria-pressed={isActive}
            onClick={() => onChange(option.value)}
          >
            {option.srLabel ? <span className="sr-only">{option.srLabel}</span> : null}
            <span aria-hidden={option.srLabel ? true : undefined}>{option.label}</span>
          </button>
        );
      })}
    </div>
  );
}
