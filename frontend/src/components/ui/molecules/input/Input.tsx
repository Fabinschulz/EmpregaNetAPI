"use client";

import * as React from "react";
import { cn } from "@/utils/lib";
import { Label } from "../../atoms/label";
import styles from "./Input.module.scss";

type Props = Omit<React.InputHTMLAttributes<HTMLInputElement>, "className"> & {
  label: string;
  error?: string | null;
  hint?: string | null;
  className?: string;
};

export function Input({ id, label, error, hint, className, ...props }: Props) {
  const inputId = id ?? props.name ?? undefined;
  const describedById = error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined;

  return (
    <div className={cn(styles.field, className)}>
      <Label htmlFor={inputId}>{label}</Label>
      <input
        {...props}
        id={inputId}
        className={styles.input}
        aria-invalid={!!error}
        aria-describedby={describedById}
      />
      {error ? (
        <div id={`${inputId}-error`} className={styles.error} role="alert">
          {error}
        </div>
      ) : hint ? (
        <div id={`${inputId}-hint`} className={styles.hint}>
          {hint}
        </div>
      ) : null}
    </div>
  );
}
