'use client';

import { Alert } from '@/components';
import type { ReactNode } from 'react';
import styles from './auth-page.module.scss';

type AuthPageProps = {
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
  apiError?: string | null;
  successMessage?: string | null;
};

export function AuthPage({ title, description, children, footer, apiError, successMessage }: AuthPageProps) {
  return (
    <section className={styles.page} aria-labelledby="auth-page-title" suppressHydrationWarning>
      <header className={styles.header}>
        <div className={styles.logo} aria-hidden>
          EU
        </div>
        <p className={styles.brand}>EmpregaUAI</p>
        <h1 id="auth-page-title" className={styles.title}>
          {title}
        </h1>
        {description ? <p className={styles.description}>{description}</p> : null}
      </header>

      <div className={styles.body}>
        {successMessage ? (
          <div className={styles.success} role="status">
            <Alert title="Sucesso">{successMessage}</Alert>
          </div>
        ) : null}
        {apiError ? (
          <div className={styles.alert}>
            <Alert variant="destructive" title="Erro">
              {apiError}
            </Alert>
          </div>
        ) : null}
        {children}
      </div>

      {footer ? <footer className={styles.footer}>{footer}</footer> : null}
    </section>
  );
}

export function AuthFormGrid({ children }: { children: ReactNode }) {
  return <div className={styles.formGrid}>{children}</div>;
}

export function AuthFormActions({ children }: { children: ReactNode }) {
  return <div className={styles.actions}>{children}</div>;
}

export function AuthDivider() {
  return (
    <p className={styles.divider} role="separator">
      ou
    </p>
  );
}

type AuthFooterPromptProps = {
  prompt: string;
  children: ReactNode;
};

export function AuthFooterPrompt({ prompt, children }: AuthFooterPromptProps) {
  return (
    <>
      <p className={styles.footerPromptText}>{prompt}</p>
      <p className={styles.footerPromptAction}>{children}</p>
    </>
  );
}
