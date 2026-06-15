export type RoleName = 'Admin' | 'Recruiter' | 'Manager' | string;

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

export function isAdmin(userRoles: readonly string[] | null | undefined): boolean {
  return hasRole(userRoles, 'Admin');
}

export function isRecruitmentStaff(userRoles: readonly string[] | null | undefined): boolean {
  return hasRole(userRoles, 'Admin') || hasRole(userRoles, 'Recruiter') || hasRole(userRoles, 'Manager');
}

export function canAccessPath(pathname: string, roles: readonly string[] | null | undefined): boolean {
  if (isPublicPath(pathname)) return true;
  if (pathname.startsWith('/recrutamento')) return isRecruitmentStaff(roles);
  if (pathname.startsWith('/admin')) return isAdmin(roles);
  return true;
}
