'use client';

import { isAdmin, isRecruitmentStaff } from '@/shared/utils/lib';
import { entityIcons } from '@/shared/components';
import type { LucideIcon } from 'lucide-react';
import { useMemo } from 'react';

export type ShellNavChild = {
  href: string;
  label: string;
  visible: boolean;
};

export type ShellNavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
  visible: boolean;
  /** Subitens do menu */
  children?: ShellNavChild[];
};

export type ShellNavGroup = {
  id: string;
  title: string;
  items: ShellNavItem[];
};

export function useAppShellNavigation(roles: string[], isAuthenticated: boolean) {
  return useMemo(() => {
    const principal: ShellNavItem[] = [
      { href: '/dashboard', label: 'Painel', icon: entityIcons.dashboard, visible: isAuthenticated },
      { href: '/vagas', label: 'Vagas', icon: entityIcons.job, visible: true },
      { href: '/candidaturas', label: 'Minhas candidaturas', icon: entityIcons.application, visible: isAuthenticated },
      {
        href: '/conta/perfil',
        label: 'Conta',
        icon: entityIcons.profile,
        visible: isAuthenticated,
        children: [
          { href: '/conta/perfil', label: 'Perfil', visible: true },
          { href: '/conta/seguranca', label: 'Segurança', visible: true }
        ]
      }
    ];

    const recruitment: ShellNavItem[] = [
      { href: '/recrutamento/vagas', label: 'Vagas (equipe)', icon: entityIcons.job, visible: isRecruitmentStaff(roles) },
      {
        href: '/recrutamento/candidaturas',
        label: 'Candidaturas',
        icon: entityIcons.application,
        visible: isRecruitmentStaff(roles)
      },
      { href: '/recrutamento/candidatos', label: 'Candidatos', icon: entityIcons.candidates, visible: isRecruitmentStaff(roles) }
    ];

    const admin: ShellNavItem[] = [
      { href: '/admin/usuarios', label: 'Usuários', icon: entityIcons.users, visible: isAdmin(roles) },
      { href: '/admin/empresas', label: 'Empresas', icon: entityIcons.company, visible: isAdmin(roles) }
    ];

    const groups: ShellNavGroup[] = [{ id: 'main', title: 'Principal', items: principal }];

    if (recruitment.some((item) => item.visible)) {
      groups.push({ id: 'recruitment', title: 'Recrutamento', items: recruitment });
    }

    if (admin.some((item) => item.visible)) {
      groups.push({ id: 'admin', title: 'Administração', items: admin });
    }

    return groups;
  }, [roles, isAuthenticated]);
}
