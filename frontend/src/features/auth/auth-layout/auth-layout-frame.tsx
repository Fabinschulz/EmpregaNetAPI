import type { ReactNode } from 'react';
import styles from '../shared/auth-layout-frame.module.scss';
import { AuthFloatingThemeToggle } from './auth-theme-toggle';

type AuthLayoutFrameProps = {
  children: ReactNode;
};

export function AuthLayoutFrame({ children }: AuthLayoutFrameProps) {
  return (
    <main className={styles.shell} suppressHydrationWarning>
      <AuthFloatingThemeToggle />
      <div className={styles.center}>{children}</div>
    </main>
  );
}
