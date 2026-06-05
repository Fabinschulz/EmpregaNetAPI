'use client';

import { isAdmin, isRecruitmentStaff } from '@/utils/lib';
import type { LucideIcon } from 'lucide-react';
import {
  Briefcase,
  Building2,
  FileText,
  LayoutDashboard,
  UserCircle,
  Users
} from 'lucide-react';
import { useMemo } from 'react';

export type ShellNavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
  visible: boolean;
};

export type ShellNavGroup = {
  id: string;
  title: string;
  items: ShellNavItem[];
};

export function useAppShellNavigation(roles: string[]) {
  return useMemo(() => {
    const principal: ShellNavItem[] = [
      { href: '/dashboard', label: 'Painel', icon: LayoutDashboard, visible: true },
      { href: '/vagas', label: 'Vagas', icon: Briefcase, visible: true },
      { href: '/candidaturas', label: 'Minhas candidaturas', icon: FileText, visible: true },
      { href: '/conta/perfil', label: 'Conta e perfil', icon: UserCircle, visible: true }
    ];

    const recruitment: ShellNavItem[] = [
      { href: '/recrutamento/vagas', label: 'Vagas (equipa)', icon: Briefcase, visible: isRecruitmentStaff(roles) },
      {
        href: '/recrutamento/candidaturas',
        label: 'Candidaturas',
        icon: FileText,
        visible: isRecruitmentStaff(roles)
      },
      { href: '/recrutamento/candidatos', label: 'Candidatos', icon: Users, visible: isRecruitmentStaff(roles) }
    ];

    const admin: ShellNavItem[] = [
      { href: '/admin/usuarios', label: 'Utilizadores', icon: Users, visible: isAdmin(roles) },
      { href: '/admin/empresas', label: 'Empresas', icon: Building2, visible: isAdmin(roles) }
    ];

    const groups: ShellNavGroup[] = [{ id: 'main', title: 'Principal', items: principal }];

    if (recruitment.some((item) => item.visible)) {
      groups.push({ id: 'recruitment', title: 'Recrutamento', items: recruitment });
    }

    if (admin.some((item) => item.visible)) {
      groups.push({ id: 'admin', title: 'Administração', items: admin });
    }

    return groups;
  }, [roles]);
}
