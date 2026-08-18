const PUBLIC_PATH_PREFIXES = [
  '/login',
  '/register',
  '/forgot-password',
  '/confirm-email',
  '/reset-password',
  '/resend-confirmation',
  '/nao-autorizado',
  '/vagas'
] as const;

export function isPublicPath(pathname: string): boolean {
  return PUBLIC_PATH_PREFIXES.some((p) => pathname === p || pathname.startsWith(`${p}/`));
}

export function hasRole(userRoles: readonly string[] | null | undefined, role: string): boolean {
  if (!userRoles || userRoles.length === 0) return false;
  return userRoles.some((r) => r.toLowerCase() === role.toLowerCase());
}

export const AUTH_POLICIES = {
  administrador: 'Administrador',
  recrutamento: 'Recrutamento'
} as const;

export type AuthPolicy = (typeof AUTH_POLICIES)[keyof typeof AUTH_POLICIES];

const POLICY_ROLES: Record<AuthPolicy, readonly string[]> = {
  Administrador: ['Admin'],
  Recrutamento: ['Admin', 'Recruiter', 'Manager']
};

export function satisfiesPolicy(userRoles: readonly string[] | null | undefined, policy: AuthPolicy): boolean {
  return POLICY_ROLES[policy].some((role) => hasRole(userRoles, role));
}

export function isAdmin(userRoles: readonly string[] | null | undefined): boolean {
  return satisfiesPolicy(userRoles, AUTH_POLICIES.administrador);
}

export function isRecruitmentStaff(userRoles: readonly string[] | null | undefined): boolean {
  return satisfiesPolicy(userRoles, AUTH_POLICIES.recrutamento);
}

export const PERMISSION_POLICIES = {
  /** `DELETE /api/companies/{id}` */
  'company.delete': AUTH_POLICIES.administrador,
  /** `DELETE /api/admin/{id}` (exclusão lógica do usuário) */
  'user.delete': AUTH_POLICIES.administrador,
  /** `DELETE /api/jobs/{id}` */
  'job.delete': AUTH_POLICIES.recrutamento,
  /** `DELETE /api/jobapplications/{id}` */
  'jobApplication.delete': AUTH_POLICIES.recrutamento
} as const;

export type Permission = keyof typeof PERMISSION_POLICIES;

export function can(userRoles: readonly string[] | null | undefined, permission: Permission): boolean {
  return satisfiesPolicy(userRoles, PERMISSION_POLICIES[permission]);
}

export function canAccessPath(pathname: string, roles: readonly string[] | null | undefined): boolean {
  if (isPublicPath(pathname)) return true;
  if (pathname.startsWith('/recrutamento')) return isRecruitmentStaff(roles);
  if (pathname.startsWith('/admin')) return isAdmin(roles);
  return true;
}
