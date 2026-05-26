'use client';

import { Button } from '@/components/ui';
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
  onToggleTheme: () => void;
  onOpenMobileMenu: () => void;
  mobileMenuOpen: boolean;
};

export function MainHeader({
  displayName,
  email,
  themeMounted,
  resolvedTheme,
  onToggleTheme,
  onOpenMobileMenu,
  mobileMenuOpen
}: MainHeaderProps) {
  const name = firstName(displayName);
  const { weekday, day, month, year } = formatGreetingDateParts();
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

        <p className={styles.greeting}>
          Olá
          {name ? (
            <>
              {' '}
              <span className={styles.accent}>{name}</span>
            </>
          ) : null}
          , hoje é {weekday},{' '}
          <span className={styles.accent}>
            {day} de {month} de {year}
          </span>
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

        <Link href="/conta/perfil" className={styles.avatarLink} title={profileTitle}>
          <UserAvatar name={displayName} />
          <span className="sr-only">Conta e perfil de {displayName}</span>
        </Link>
      </div>
    </header>
  );
}
