import type { ReactNode } from 'react';
import { AuthFloatingThemeToggle } from './auth-theme-toggle';

type AuthLayoutFrameProps = {
  children: ReactNode;
};

export function AuthLayoutFrame({ children }: AuthLayoutFrameProps) {
  return (
    <main className="container">
      <AuthFloatingThemeToggle />
      {children}
    </main>
  );
}
