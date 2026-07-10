import { canAccessPath, isPublicPath } from './rbac';

export const LOGIN_PATH = '/login';
export const FORBIDDEN_REDIRECT_PATH = '/nao-autorizado';
export const DEFAULT_POST_LOGIN_PATH = '/dashboard';

export type RouteAccessDecision = 'allow' | 'login' | 'forbidden';

export type RouteSession = {
  isAuthenticated: boolean;
  roles: readonly string[] | null | undefined;
};

/** Path interno seguro (rejeita protocol-relative `//`). */
export function isSafeInternalPath(path: string | null | undefined): path is string {
  return !!path && path.startsWith('/') && !path.startsWith('//');
}

/** Política única de acesso a rotas (edge proxy + guards cliente). */
export function evaluateRouteAccess(pathname: string, session: RouteSession): RouteAccessDecision {
  if (isPublicPath(pathname)) return 'allow';
  if (!session.isAuthenticated) return 'login';
  if (!canAccessPath(pathname, session.roles)) return 'forbidden';
  return 'allow';
}

export function isRouteAccessAllowed(pathname: string, session: RouteSession): boolean {
  return evaluateRouteAccess(pathname, session) === 'allow';
}

export function buildLoginRedirectPath(returnPath: string): string {
  return `${LOGIN_PATH}?redirect=${encodeURIComponent(returnPath)}`;
}

export function buildForbiddenRedirectPath(fromPath?: string): string {
  if (!isSafeInternalPath(fromPath)) {
    return FORBIDDEN_REDIRECT_PATH;
  }
  return `${FORBIDDEN_REDIRECT_PATH}?from=${encodeURIComponent(fromPath)}`;
}

export function resolvePostLoginPath(searchParams: URLSearchParams | null | undefined): string {
  const redirectUrl = searchParams?.get('redirect');
  if (isSafeInternalPath(redirectUrl)) return redirectUrl;
  return DEFAULT_POST_LOGIN_PATH;
}
