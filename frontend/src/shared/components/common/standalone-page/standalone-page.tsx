'use client';

import { Alert } from '@/components';
import type { ReactNode } from 'react';
import { EmpregaUaiLogo } from '../branding';
import styles from './standalone-page.module.scss';

type StandalonePageProps = {
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
  apiError?: string | null;
  successMessage?: string | null;
};

/**
 * Shell de página autônoma (fora do AppShell): auth (login, registro, senha...)
 * e páginas de estado (ex.: não autorizado). Cabeçalho com marca + título,
 * corpo com alertas de sucesso/erro e rodapé opcional.
 */
export function StandalonePage({
  title,
  description,
  children,
  footer,
  apiError,
  successMessage
}: StandalonePageProps) {
  return (
    <section className={styles.page} aria-labelledby="auth-page-title" suppressHydrationWarning>
      <header className={styles.header}>
        <EmpregaUaiLogo />
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

export function StandalonePageFormGrid({ children }: { children: ReactNode }) {
  return <div className={styles.formGrid}>{children}</div>;
}

export function StandalonePageFormActions({ children }: { children: ReactNode }) {
  return <div className={styles.actions}>{children}</div>;
}

export function StandalonePageDivider() {
  return (
    <p className={styles.divider} role="separator">
      ou
    </p>
  );
}

type StandalonePageFooterPromptProps = {
  prompt: string;
  children: ReactNode;
};

export function StandalonePageFooterPrompt({ prompt, children }: StandalonePageFooterPromptProps) {
  return (
    <>
      <p className={styles.footerPromptText}>{prompt}</p>
      <p className={styles.footerPromptAction}>{children}</p>
    </>
  );
}
