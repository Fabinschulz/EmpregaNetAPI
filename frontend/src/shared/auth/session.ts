import { parseCookieHeader } from '@/utils';
import { jwtDecode } from 'jwt-decode';

/**
 * Nome do cookie httpOnly de access token emitido pelo backend.
 * No host `localhost` o cookie é compartilhado entre portas, então o middleware/proxy (server-side)
 * consegue lê-lo para o gating de rotas. Nunca é legível por JS no cliente (httpOnly).
 */
export const ACCESS_TOKEN_COOKIE = 'access_token';

type JwtPayload = {
  exp?: number;
  roles?: string[];
};

export type Session = {
  token: string;
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

function buildSession(token: string): Session {
  const normalized = normalizeBearer(token);
  const { username, email } = decodeUserDisplayFromJwt(normalized);
  return {
    token: normalized,
    roles: decodeRolesFromJwt(normalized),
    exp: decodeExp(normalized),
    username,
    email
  };
}

/**
 * Constrói a sessão a partir do cookie httpOnly `access_token` presente no header `Cookie`.
 * Uso exclusivamente server-side (middleware/proxy) para gating de rotas.
 */
export function readSessionFromCookieHeader(cookieHeader: string | null | undefined): Session | null {
  const cookies = parseCookieHeader(cookieHeader);
  const token = cookies[ACCESS_TOKEN_COOKIE];
  if (!token) return null;
  return buildSession(token);
}

// ---------------------------------------------------------------------------
// Metadados de sessão no cliente (hidratação sem requisição).
//
// Guarda APENAS dados de exibição/gating de UI (roles, nome, e-mail), nunca o token.
// A credencial vive exclusivamente nos cookies httpOnly: um XSS que leia estes
// metadados não obtém nada utilizável, pois a autorização real é decidida no
// servidor pelo cookie. Se os metadados ficarem obsoletos (logout noutra aba,
// cookies expirados), a primeira chamada à API devolve 401 e o interceptor corrige.
// ---------------------------------------------------------------------------

const SESSION_METADATA_KEY = 'empreganet_session_meta';
const SESSION_METADATA_EVENT = 'empreganet:session-meta';

export type SessionMetadata = {
  roles: string[];
  username: string | null;
  email: string | null;
};

function parseMetadata(raw: string): SessionMetadata | null {
  try {
    const parsed = JSON.parse(raw) as SessionMetadata;
    if (!Array.isArray(parsed.roles)) return null;
    return parsed;
  } catch {
    return null;
  }
}

// Cache referencial para o snapshot do useSyncExternalStore (mesmo raw -> mesmo objeto).
let metadataCacheRaw: string | null = null;
let metadataCacheParsed: SessionMetadata | null = null;

/** Snapshot dos metadados de sessão (estável referencialmente para useSyncExternalStore). */
export function getSessionMetadataSnapshot(): SessionMetadata | null {
  const raw = localStorage.getItem(SESSION_METADATA_KEY);
  if (raw !== metadataCacheRaw) {
    metadataCacheRaw = raw;
    metadataCacheParsed = raw ? parseMetadata(raw) : null;
  }
  return metadataCacheParsed;
}

/**
 * Subscreve mudanças dos metadados: evento custom (mesma aba) + `storage` (outras abas),
 * o que sincroniza login/logout entre abas automaticamente.
 */
export function subscribeSessionMetadata(callback: () => void): () => void {
  window.addEventListener(SESSION_METADATA_EVENT, callback);
  window.addEventListener('storage', callback);
  return () => {
    window.removeEventListener(SESSION_METADATA_EVENT, callback);
    window.removeEventListener('storage', callback);
  };
}

function notifySessionMetadataChanged() {
  window.dispatchEvent(new Event(SESSION_METADATA_EVENT));
}

export function saveSessionMetadata(meta: SessionMetadata) {
  if (typeof window === 'undefined') return;
  localStorage.setItem(SESSION_METADATA_KEY, JSON.stringify(meta));
  notifySessionMetadataChanged();
}

export function clearSessionMetadata() {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(SESSION_METADATA_KEY);
  notifySessionMetadataChanged();
}
