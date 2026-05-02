"use client";

import type { LucideIcon } from "lucide-react";
import { RefreshCw } from "lucide-react";
import { Button } from "@/components/ui";
import styles from "./error-fallback.module.scss";

export type ErrorFallbackProps = {
  statusCode: number;
  title: string;
  message?: string;
  buttonText?: string;
  Icon?: LucideIcon;
  onButtonClick: () => void;
};

export function ErrorFallback({
  statusCode,
  title,
  message,
  buttonText = "Tentar novamente",
  Icon = RefreshCw,
  onButtonClick,
}: ErrorFallbackProps) {
  return (
    <div className={styles.root}>
      <div className={styles.inner}>
        <span className={styles.code}>{statusCode}</span>
        <div className={styles.copy}>
          <h1 className={styles.title}>{title}</h1>
          {message ? <p className={styles.message}>{message}</p> : null}
          <Button type="button" variant="primary" onClick={onButtonClick} startIcon={Icon}>
            {buttonText}
          </Button>
        </div>
      </div>
    </div>
  );
}
