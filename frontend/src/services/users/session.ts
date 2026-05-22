
import { deleteClientCookie, parseCookieHeader, setClientCookie } from '@/utils/lib';
import { jwtDecode } from 'jwt-decode';


export const AUTH_COOKIE = 'empreganet_access_token';
export const REFRESH_COOKIE = 'empreganet_refresh_token';

type JwtPayload = {
  exp?: number;
  roles?: string[];
};

export type Session = {
  token: string;
  refreshToken?: string | null;
  roles: string[];
  exp?: number;
  username: string | null;
  email: string | null;
};

export function normalizeBearer(token: string): string {
  return token.startsWith('Bearer ') ? token : `Bearer ${token}`;
}

export function stripBearer(token: string): string {
  return token.startsWith('Bearer ') ? token.slice('Bearer '.length) : token;
}

export function decodeRolesFromJwt(token: string): string[] {
  try {
    const raw = stripBearer(token);
    const payload = jwtDecode<Record<string, unknown>>(raw);
    const roles = payload['role'];
    if (typeof roles === 'string') return [roles];
    if (Array.isArray(roles) && roles.every((r) => typeof r === 'string')) return roles;
    return [];
  } catch {
    return [];
  }
}

export function decodeExp(token: string): number | undefined {
  try {
    const raw = stripBearer(token);
    const payload = jwtDecode<JwtPayload>(raw);
    return payload.exp;
  } catch {
    return undefined;
  }
}

const EMAIL_CLAIM_URIS = [
  'email',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
] as const;

const USERNAME_CLAIM_KEYS = [
  'userName',
  'unique_name',
  'name',
  'preferred_username',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
] as const;

/** Lê nome e e-mail embutidos no JWT (claims usadas pelo backend EmpregaNet). */
export function decodeUserDisplayFromJwt(token: string): { username: string | null; email: string | null } {
  const str = (v: unknown) => (typeof v === 'string' && v.trim().length > 0 ? v.trim() : null);
  try {
    const raw = stripBearer(token);
    const payload = jwtDecode<Record<string, unknown>>(raw);
    let email: string | null = null;
    for (const k of EMAIL_CLAIM_URIS) {
      email = str(payload[k]);
      if (email) break;
    }
    let username: string | null = null;
    for (const k of USERNAME_CLAIM_KEYS) {
      username = str(payload[k]);
      if (username) break;
    }
    return { username, email };
  } catch {
    return { username: null, email: null };
  }
}

export function saveSessionClient(params: { token: string; refreshToken?: string | null }) {
  const token = normalizeBearer(params.token);
  setClientCookie(AUTH_COOKIE, token);
  if (params.refreshToken) setClientCookie(REFRESH_COOKIE, params.refreshToken);
}

export function clearSessionClient() {
  deleteClientCookie(AUTH_COOKIE);
  deleteClientCookie(REFRESH_COOKIE);
}

export function readSessionFromCookieHeader(cookieHeader: string | null | undefined): Session | null {
  const cookies = parseCookieHeader(cookieHeader);
  const token = cookies[AUTH_COOKIE];
  if (!token) return null;
  const refreshToken = cookies[REFRESH_COOKIE];
  const { username, email } = decodeUserDisplayFromJwt(token);
  return {
    token,
    refreshToken,
    roles: decodeRolesFromJwt(token),
    exp: decodeExp(token),
    username,
    email
  };
}

/** Sessão a partir de `document.cookie` (apenas browser). */
export function readSessionFromBrowser(): Session | null {
  if (typeof document === 'undefined') return null;
  return readSessionFromCookieHeader(document.cookie);
}
