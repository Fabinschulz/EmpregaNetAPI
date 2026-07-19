import type { ReactNode } from 'react';
import { FloatingThemeToggle } from './FloatingThemeToggle';
import styles from './page-frame.module.scss';

type CenteredPageFrameProps = {
  children: ReactNode;
};

/**
 * Moldura de página centralizada (auth, páginas de estado): fundo do app,
 * conteúdo ao centro e alternador de tema flutuante.
 */
export function CenteredPageFrame({ children }: CenteredPageFrameProps) {
  return (
    <main className={styles.shell} suppressHydrationWarning>
      <FloatingThemeToggle />
      <div className={styles.center}>{children}</div>
    </main>
  );
}
