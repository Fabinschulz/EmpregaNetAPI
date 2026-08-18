import { parseCookieHeader } from '@/shared/utils';
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
/**
 * A sessão existe e ainda não expirou.
 *
 * Mora aqui, e não inline no `proxy.ts`, porque é a regra que decide se uma requisição é tratada
 * como autenticada — a coisa mais próxima de uma decisão de segurança que o frontend toma. Como
 * função nomeada ela é coberta por cenários; como expressão solta dentro do middleware, não era.
 *
 * Um token sem `exp` é aceito: a API é quem valida a assinatura e o prazo de verdade, e recusar
 * aqui um token que o servidor aceitaria só produziria um logout que o usuário não entende.
 */
export function isSessionValid(session: Session | null): boolean {
  if (!session?.token) return false;
  if (session.exp === undefined) return true;
  return session.exp * 1000 > Date.now();
}

export function readSessionFromCookieHeader(cookieHeader: string | null | undefined): Session | null {
  const cookies = parseCookieHeader(cookieHeader);
  const token = cookies[ACCESS_TOKEN_COOKIE];
  if (!token) return null;
  return buildSession(token);
}
