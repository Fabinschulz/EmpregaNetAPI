'use client';

import { useAuth, useTheme } from '@/context';
import { useHasMounted } from '@/hooks';
import { startRouterTransition, toastSuccess } from '@/utils';
import clsx from 'clsx';
import { usePathname, useRouter } from 'next/navigation';
import { useCallback, useEffect, useState } from 'react';
import styles from './AppShell.module.scss';
import { useAppShellNavigation } from './hooks/use-app-shell-navigation';
import { MainHeader } from './main-header';
import { MobileNav } from './MobileNav';
import { Sidebar } from './sidebar';

/**
 * Moldura única da aplicação (sidebar + header + conteúdo), usada tanto nas rotas
 * autenticadas quanto nas públicas (catálogo de vagas).
 *
 * Adapta-se à sessão: sem autenticação, o menu mostra apenas o que é utilizável sem login,
 * a saudação omite o nome e as ações de conta viram "Entrar".
 *
 * Não bloqueia o `children`: ele é renderizado incondicionalmente, então conteúdo vindo de
 * Server Components continua a ser renderizado no servidor (SSR/SEO preservados).
 */
export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { roles, logout, username, email, isAuthenticated } = useAuth();
  const { resolvedTheme, setTheme } = useTheme();
  const themeMounted = useHasMounted();

  const [mobileOpen, setMobileOpen] = useState(false);
  const [prevPathname, setPrevPathname] = useState(pathname);
  const [railCollapsed, setRailCollapsed] = useState(false);
  const [railTransitioning, setRailTransitioning] = useState(false);

  if (pathname !== prevPathname) {
    setPrevPathname(pathname);
    if (mobileOpen) {
      setMobileOpen(false);
    }
  }

  const displayName = username?.trim() || email?.trim() || 'Usuário';
  const navGroups = useAppShellNavigation(roles, isAuthenticated);

  const closeMobile = useCallback(() => setMobileOpen(false), []);

  const toggleLightDark = () => {
    if (!themeMounted) return;
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark');
  };

  const toggleRailCollapsed = useCallback(() => {
    setRailTransitioning(true);
    setRailCollapsed((value) => !value);
  }, []);

  useEffect(() => {
    if (!railTransitioning) return;
    const timer = window.setTimeout(() => setRailTransitioning(false), 260);
    return () => window.clearTimeout(timer);
  }, [railCollapsed, railTransitioning]);

  const handleLogout = useCallback(async () => {
    await logout();
    toastSuccess('Sessão terminada', 'Terminou sessão com segurança. Até breve.');
    startRouterTransition(() => router.push('/login'));
  }, [logout, router]);

  return (
    <div className={clsx(styles.shell, railCollapsed && styles.shellCollapsed)}>
      <a href="#conteudo" className={styles.skipLink}>
        Pular para o conteúdo
      </a>

      <Sidebar
        className={styles.sidebarDesktop}
        groups={navGroups}
        collapsed={railCollapsed}
        mobileOpen={false}
        railTransitioning={railTransitioning}
        isAuthenticated={isAuthenticated}
        onNavigate={closeMobile}
        onCloseMobile={closeMobile}
        onToggleCollapsed={toggleRailCollapsed}
        onLogout={handleLogout}
      />

      <MobileNav
        open={mobileOpen}
        onOpenChange={setMobileOpen}
        groups={navGroups}
        isAuthenticated={isAuthenticated}
        onNavigate={closeMobile}
        onLogout={handleLogout}
      />

      <section className={styles.content}>
        <MainHeader
          displayName={displayName}
          email={email}
          themeMounted={themeMounted}
          resolvedTheme={resolvedTheme}
          isAuthenticated={isAuthenticated}
          onToggleTheme={toggleLightDark}
          onOpenMobileMenu={() => setMobileOpen(true)}
          mobileMenuOpen={mobileOpen}
        />

        <main id="conteudo" className={styles.main} suppressHydrationWarning>
          {children}
        </main>
      </section>
    </div>
  );
}
