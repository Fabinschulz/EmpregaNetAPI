'use client';

import { Button } from '@/components/ui';
import { useTheme } from '@/context';
import { useAuth } from '@/features/auth';
import { isAdmin, isRecruitmentStaff, startRouterTransition, toastSuccess } from '@/utils/lib';
import clsx from 'clsx';
import type { LucideIcon } from 'lucide-react';
import {
    Briefcase,
    Building2,
    FileText,
    LayoutDashboard,
    Menu,
    Moon,
    PanelLeft,
    PanelLeftClose,
    Sun,
    UserCircle,
    Users,
    X
} from 'lucide-react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useHasMounted } from '@/hooks/use-has-mounted';
import styles from './AppShell.module.scss';

type NavItem = { href: string; label: string; icon: LucideIcon; visible: boolean };

type NavGroup = { id: string; title: string; items: NavItem[] };

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { roles, logout, username, email } = useAuth();
  const { resolvedTheme, setTheme } = useTheme();
  const themeMounted = useHasMounted();
  const toggleLightDark = () => {
    if (!themeMounted) return;
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark');
  };
  const [mobileOpen, setMobileOpen] = useState(false);
  const [railCollapsed, setRailCollapsed] = useState(false);

  const displayName = username?.trim() || email?.trim() || 'Usuário';
  const displaySub = username?.trim() && email?.trim() && username !== email ? email : null;

  const navGroups: NavGroup[] = useMemo(() => {
    const principal: NavItem[] = [
      { href: '/dashboard', label: 'Painel', icon: LayoutDashboard, visible: true },
      { href: '/vagas', label: 'Vagas', icon: Briefcase, visible: true },
      { href: '/candidaturas', label: 'Minhas candidaturas', icon: FileText, visible: true },
      { href: '/conta/perfil', label: 'Conta e perfil', icon: UserCircle, visible: true }
    ];
    const recruitment: NavItem[] = [
      { href: '/recrutamento/vagas', label: 'Vagas (equipa)', icon: Briefcase, visible: isRecruitmentStaff(roles) },
      {
        href: '/recrutamento/candidaturas',
        label: 'Candidaturas',
        icon: FileText,
        visible: isRecruitmentStaff(roles)
      },
      { href: '/recrutamento/candidatos', label: 'Candidatos', icon: Users, visible: isRecruitmentStaff(roles) }
    ];
    const admin: NavItem[] = [
      { href: '/admin/usuarios', label: 'Utilizadores', icon: Users, visible: isAdmin(roles) },
      { href: '/admin/empresas', label: 'Empresas', icon: Building2, visible: isAdmin(roles) }
    ];
    const groups: NavGroup[] = [{ id: 'main', title: 'Principal', items: principal }];
    if (recruitment.some((i) => i.visible)) {
      groups.push({ id: 'recruitment', title: 'Recrutamento', items: recruitment });
    }
    if (admin.some((i) => i.visible)) {
      groups.push({ id: 'admin', title: 'Administração', items: admin });
    }
    return groups;
  }, [roles]);

  const closeMobile = useCallback(() => setMobileOpen(false), []);

  useEffect(() => {
    closeMobile();
  }, [pathname, closeMobile]);

  const handleLogout = useCallback(() => {
    logout();
    toastSuccess('Sessão terminada', 'Terminou sessão com segurança. Até breve.');
    startRouterTransition(() => router.push('/login'));
  }, [logout, router]);

  const sidebarInner = (
    <>
      <div className={styles.sidebarHeader}>
        <Link href="/dashboard" className={styles.brand} onClick={closeMobile}>
          EmpregaNet
        </Link>
        <div className={styles.sidebarHeaderActions}>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className={styles.desktopRailToggle}
            aria-label={railCollapsed ? 'Expandir menu lateral' : 'Compactar menu lateral'}
            onClick={() => setRailCollapsed((v) => !v)}
          >
            {railCollapsed ? <PanelLeft className={styles.headerIcon} /> : <PanelLeftClose className={styles.headerIcon} />}
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className={styles.mobileClose}
            aria-label="Fechar menu"
            onClick={closeMobile}
          >
            <X className={styles.headerIcon} />
          </Button>
        </div>
      </div>

      <nav
        className={styles.sidebarScroll}
        aria-label="Navegação principal"
        suppressHydrationWarning
      >
        {navGroups.map((group) => {
          const visibleItems = group.items.filter((i) => i.visible);
          if (visibleItems.length === 0) return null;
          return (
            <div key={group.id} className={styles.navGroup}>
              {!railCollapsed ? <div className={styles.navGroupTitle}>{group.title}</div> : null}
              <div className={styles.navList}>
                {visibleItems.map((item) => {
                  const active = pathname === item.href || pathname?.startsWith(item.href + '/');
                  const Icon = item.icon;
                  return (
                    <Button
                      key={item.href}
                      asChild
                      variant="ghost"
                      className={clsx(styles.navButton, styles.link, active && styles.active, railCollapsed && styles.linkIconOnly)}
                    >
                      <Link href={item.href} onClick={closeMobile} title={railCollapsed ? item.label : undefined}>
                        <Icon className={styles.navIcon} aria-hidden />
                        {!railCollapsed ? <span className={styles.navLabel}>{item.label}</span> : null}
                      </Link>
                    </Button>
                  );
                })}
              </div>
            </div>
          );
        })}
      </nav>

      <div className={styles.sidebarFooter}>
        <div className={styles.userBlock} title={displaySub ?? undefined}>
          <div className={styles.userAvatar} aria-hidden>
            {(displayName.slice(0, 1) || '?').toUpperCase()}
          </div>
          {!railCollapsed ? (
            <div className={styles.userText}>
              <div className={styles.userName}>{displayName}</div>
              {displaySub ? <div className={styles.userEmail}>{displaySub}</div> : null}
            </div>
          ) : null}
        </div>
        <Button variant="outline" size="sm" className={styles.logoutBtn} onClick={handleLogout}>
          Sair
        </Button>
      </div>
    </>
  );

  return (
    <div className={clsx(styles.shell, railCollapsed && styles.shellCollapsed)}>
      <div
        className={clsx(styles.overlay, mobileOpen && styles.overlayVisible)}
        aria-hidden={!mobileOpen}
        onClick={closeMobile}
      />

      <aside
        id="app-sidebar"
        className={clsx(styles.sidebar, railCollapsed && styles.sidebarCollapsed, mobileOpen && styles.sidebarMobileOpen)}
        suppressHydrationWarning
      >
        {sidebarInner}
      </aside>

      <section className={styles.content}>
        <header className={styles.topbar}>
          <div className={styles.topbarLeft}>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className={styles.menuToggle}
              aria-label="Abrir menu de navegação"
              aria-expanded={mobileOpen}
              aria-controls="app-sidebar"
              onClick={() => setMobileOpen(true)}
            >
              <Menu className={styles.headerIcon} />
            </Button>
            <div className={styles.topbarContext}>
              <span className={styles.topbarEyebrow}>Área autenticada</span>
              <span className={styles.topbarHint}>Utilize o menu para aceder às áreas do portal.</span>
            </div>
          </div>
          <div className={styles.topbarRight}>
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
              onClick={toggleLightDark}
            >
              {themeMounted && resolvedTheme === 'dark' ? (
                <Sun className={styles.headerIcon} />
              ) : (
                <Moon className={styles.headerIcon} />
              )}
            </Button>
          </div>
        </header>

        <div className={styles.main}>{children}</div>
      </section>
    </div>
  );
}
