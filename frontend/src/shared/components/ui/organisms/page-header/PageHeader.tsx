import { cn } from '@/utils';
import * as React from 'react';
import styles from './PageHeader.module.scss';

export type PageHeaderProps = {
  title: React.ReactNode;
  description?: React.ReactNode;
  actions?: React.ReactNode;
  className?: string;
};

/**
 * Cabeçalho padrão de página: título + descrição à esquerda, ações à direita.
 * Mantém todas as páginas com a mesma hierarquia visual, sem estilos inline.
 */
export function PageHeader({ title, description, actions, className }: PageHeaderProps) {
  return (
    <header className={cn(styles.root, className)}>
      <div>
        <h1 className={styles.title}>{title}</h1>
        {description ? <p className={styles.description}>{description}</p> : null}
      </div>
      {actions ? <div className={styles.actions}>{actions}</div> : null}
    </header>
  );
}
