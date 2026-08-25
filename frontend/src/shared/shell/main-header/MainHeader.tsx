'use client';

import { actionIcons, Button, IconButton } from '@/shared/components';
import { useHasMounted } from '@/shared/hooks';
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
        <IconButton
          icon={actionIcons.menu}
          label="Abrir menu de navegação"
          showTooltip={false}
          className={styles.menuToggle}
          iconStyleOverrides={styles.headerIcon}
          aria-expanded={mobileMenuOpen}
          aria-controls="app-sidebar"
          onClick={onOpenMobileMenu}
        />

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
        <IconButton
          icon={themeMounted && resolvedTheme === 'dark' ? actionIcons.themeLight : actionIcons.themeDark}
          label={
            !themeMounted
              ? 'Alternar tema'
              : resolvedTheme === 'dark'
                ? 'Alternar para tema claro'
                : 'Alternar para tema escuro'
          }
          iconStyleOverrides={styles.headerIcon}
          onClick={onToggleTheme}
        />

        {isAuthenticated ? (
          <Link href="/conta/perfil" className={styles.avatarLink} title={profileTitle}>
            <UserAvatar name={displayName} />
            <span className="sr-only">Conta e perfil de {displayName}</span>
          </Link>
        ) : (
          <Button variant="primary" size="sm" asChild>
            <Link href="/login">
              <actionIcons.signIn aria-hidden />
              Entrar
            </Link>
          </Button>
        )}
      </div>
    </header>
  );
}
