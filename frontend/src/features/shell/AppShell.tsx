"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import clsx from "clsx";
import styles from "./AppShell.module.scss";
import { Button } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { isAdmin, isRecruitmentStaff } from "@/utils/lib";

type NavItem = { href: string; label: string; visible: boolean };

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { roles, logout } = useAuth();

  const items: NavItem[] = [
    { href: "/dashboard", label: "Dashboard", visible: true },
    { href: "/vagas", label: "Vagas", visible: true },
    { href: "/candidaturas", label: "Minhas candidaturas", visible: true },
    { href: "/conta/perfil", label: "Minha conta", visible: true },
    { href: "/recrutamento/vagas", label: "Recrutamento: Vagas", visible: isRecruitmentStaff(roles) },
    { href: "/recrutamento/candidaturas", label: "Recrutamento: Candidaturas", visible: isRecruitmentStaff(roles) },
    { href: "/recrutamento/candidatos", label: "Recrutamento: Candidatos", visible: isRecruitmentStaff(roles) },
    { href: "/admin/usuarios", label: "Admin: Usuários", visible: isAdmin(roles) },
    { href: "/admin/empresas", label: "Admin: Empresas", visible: isAdmin(roles) }
  ];

  return (
    <div className={styles.shell}>
      <aside className={styles.sidebar}>
        <div className={styles.brand}>EmpregaNet</div>
        <nav className={styles.nav} aria-label="Navegação principal">
          {items
            .filter((i) => i.visible)
            .map((item) => {
              const active = pathname === item.href || pathname?.startsWith(item.href + "/");
              return (
                <Button
                  key={item.href}
                  asChild
                  variant="ghost"
                  className={clsx(styles.navButton, styles.link, active && styles.active)}
                >
                  <Link href={item.href}>{item.label}</Link>
                </Button>
              );
            })}
        </nav>
      </aside>
      <section className={styles.content}>
        <div className={styles.topbar}>
          <div className={styles.user}>Sessão ativa</div>
          <Button variant="ghost" onClick={logout}>
            Sair
          </Button>
        </div>
        <div className={styles.main}>{children}</div>
      </section>
    </div>
  );
}

