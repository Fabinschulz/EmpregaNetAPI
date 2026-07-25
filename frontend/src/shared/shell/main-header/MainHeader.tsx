'use client';

import { Button } from '@/components/ui';
import { useHasMounted } from '@/hooks';
import { Menu, Moon, Sun } from 'lucide-react';
import Link from 'next/link';
import { UserAvatar } from '../user-avatar';
import { firstName, formatGreetingDateParts } from '../utils/greeting';
import styles from './main-header.module.scss';

type MainHeaderProps = {
  displayName: string;
  email: string | null;
  themeMounted: boolean;
  resolvedTheme: string | undefined;
  isAuthenticated: boolean;
  onToggleTheme: () => void;
  onOpenMobileMenu: () => void;
  mobileMenuOpen: boolean;
};

export function MainHeader({
  displayName,
  email,
  themeMounted,
  resolvedTheme,
  isAuthenticated,
  onToggleTheme,
  onOpenMobileMenu,
  mobileMenuOpen
}: MainHeaderProps) {
  const name = isAuthenticated ? firstName(displayName) : undefined;
  const mounted = useHasMounted();
  const date = mounted ? formatGreetingDateParts() : null;
  const profileTitle = email?.trim() || displayName;

  return (
    <header className={styles.topbar}>
      <div className={styles.left}>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className={styles.menuToggle}
          aria-label="Abrir menu de navegação"
          aria-expanded={mobileMenuOpen}
          aria-controls="app-sidebar"
          onClick={onOpenMobileMenu}
        >
          <Menu className={styles.headerIcon} aria-hidden />
        </Button>

        <p className={styles.greeting} suppressHydrationWarning>
          Olá
          {name ? (
            <>
              {' '}
              <span className={styles.accent}>{name}</span>
            </>
          ) : null}
          {date ? (
            <>
              , hoje é {date.weekday},{' '}
              <span className={styles.accent}>
                {date.day} de {date.month} de {date.year}
              </span>
            </>
          ) : null}
        </p>
      </div>

      <div className={styles.right}>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label={
            !themeMounted
              ? 'Alternar tema'
              : resolvedTheme === 'dark'
                ? 'Alternar para tema claro'
                : 'Alternar para tema escuro'
          }
          onClick={onToggleTheme}
        >
          {themeMounted && resolvedTheme === 'dark' ? (
            <Sun className={styles.headerIcon} aria-hidden />
          ) : (
            <Moon className={styles.headerIcon} aria-hidden />
          )}
        </Button>

        {isAuthenticated ? (
          <Link href="/conta/perfil" className={styles.avatarLink} title={profileTitle}>
            <UserAvatar name={displayName} />
            <span className="sr-only">Conta e perfil de {displayName}</span>
          </Link>
        ) : (
          <Button variant="primary" size="sm" asChild>
            <Link href="/login">Entrar</Link>
          </Button>
        )}
      </div>
    </header>
  );
}
