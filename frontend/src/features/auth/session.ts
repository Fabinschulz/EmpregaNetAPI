import { jwtDecode } from "jwt-decode";
import { AUTH_COOKIE, REFRESH_COOKIE } from "@/services";
import { deleteClientCookie, parseCookieHeader, setClientCookie } from "@/utils/lib";

type JwtPayload = {
  exp?: number;
  roles?: string[];
};

export type Session = {
  token: string;
  refreshToken?: string | null;
  roles: string[];
  exp?: number;
};

export function normalizeBearer(token: string): string {
  return token.startsWith("Bearer ") ? token : `Bearer ${token}`;
}

export function stripBearer(token: string): string {
  return token.startsWith("Bearer ") ? token.slice("Bearer ".length) : token;
}

export function decodeRolesFromJwt(token: string): string[] {
  try {
    const raw = stripBearer(token);
    const payload = jwtDecode<Record<string, unknown>>(raw);
    const roles = payload["role"];
    if (typeof roles === "string") return [roles];
    if (Array.isArray(roles) && roles.every((r) => typeof r === "string")) return roles;
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
  return {
    token,
    refreshToken,
    roles: decodeRolesFromJwt(token),
    exp: decodeExp(token),
  };
}

