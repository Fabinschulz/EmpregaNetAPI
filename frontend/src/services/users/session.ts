import { deleteClientCookie, parseCookieHeader, setClientCookie } from '@/utils';
import { jwtDecode } from 'jwt-decode';

export const AUTH_COOKIE = 'empreganet_access_token';
export const REFRESH_COOKIE = 'empreganet_refresh_token';
const SESSION_STORAGE_KEY = 'empreganet_session';

type JwtPayload = {
  exp?: number;
  roles?: string[];
};

type StoredSession = {
  token: string;
  refreshToken?: string | null;
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

const EMAIL_CLAIM_URIS = ['email', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] as const;

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

function readStoredSession(): StoredSession | null {
  if (typeof window === 'undefined') return null;
  try {
    const raw = sessionStorage.getItem(SESSION_STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as StoredSession;
    if (!parsed?.token) return null;
    return parsed;
  } catch {
    return null;
  }
}

function writeStoredSession(session: StoredSession) {
  if (typeof window === 'undefined') return;
  sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
}

function clearStoredSession() {
  if (typeof window === 'undefined') return;
  sessionStorage.removeItem(SESSION_STORAGE_KEY);
}

function buildSession(token: string, refreshToken?: string | null): Session {
  const normalized = normalizeBearer(token);
  const { username, email } = decodeUserDisplayFromJwt(normalized);
  return {
    token: normalized,
    refreshToken,
    roles: decodeRolesFromJwt(normalized),
    exp: decodeExp(normalized),
    username,
    email
  };
}

export function saveSessionClient(params: { token: string; refreshToken?: string | null }) {
  const token = normalizeBearer(params.token);
  writeStoredSession({ token, refreshToken: params.refreshToken ?? null });
  setClientCookie(AUTH_COOKIE, token);
  if (params.refreshToken) setClientCookie(REFRESH_COOKIE, params.refreshToken, { maxAgeSeconds: 60 * 60 * 24 * 14 });
}

export function clearSessionClient() {
  clearStoredSession();
  deleteClientCookie(AUTH_COOKIE);
  deleteClientCookie(REFRESH_COOKIE);
}

export function readSessionFromCookieHeader(cookieHeader: string | null | undefined): Session | null {
  const cookies = parseCookieHeader(cookieHeader);
  const token = cookies[AUTH_COOKIE];
  if (!token) return null;
  const refreshToken = cookies[REFRESH_COOKIE];
  return buildSession(token, refreshToken);
}

export function readSessionFromBrowser(): Session | null {
  if (typeof document === 'undefined') return null;
  const stored = readStoredSession();
  if (stored?.token) return buildSession(stored.token, stored.refreshToken);
  return readSessionFromCookieHeader(document.cookie);
}

export function readRefreshTokenFromBrowser(): string | null {
  const session = readSessionFromBrowser();
  return session?.refreshToken ?? null;
}
