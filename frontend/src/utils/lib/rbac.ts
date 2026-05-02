export type RoleName = "Admin" | "Recruiter" | "Manager" | string;

export function hasRole(userRoles: readonly string[] | null | undefined, role: string): boolean {
  if (!userRoles || userRoles.length === 0) return false;
  return userRoles.some((r) => r.toLowerCase() === role.toLowerCase());
}

export function isAdmin(userRoles: readonly string[] | null | undefined): boolean {
  return hasRole(userRoles, "Admin");
}

export function isRecruitmentStaff(userRoles: readonly string[] | null | undefined): boolean {
  return (
    hasRole(userRoles, "Admin") ||
    hasRole(userRoles, "Recruiter") ||
    hasRole(userRoles, "Manager")
  );
}

export function canAccessPath(pathname: string, roles: readonly string[] | null | undefined): boolean {
  if (pathname.startsWith("/login") || pathname.startsWith("/register")) return true;
  if (pathname.startsWith("/recrutamento")) return isRecruitmentStaff(roles);
  if (pathname.startsWith("/admin")) return isAdmin(roles);

  return true;
}

