function isProduction(): boolean {
  return process.env.NODE_ENV === 'production';
}

export function parseCookieHeader(cookieHeader: string | null | undefined): Record<string, string> {
  if (!cookieHeader) return {};
  return cookieHeader
    .split(';')
    .map((p) => p.trim())
    .filter(Boolean)
    .reduce<Record<string, string>>((acc, part) => {
      const idx = part.indexOf('=');
      if (idx === -1) return acc;
      const k = decodeURIComponent(part.slice(0, idx).trim());
      const v = decodeURIComponent(part.slice(idx + 1).trim());
      acc[k] = v;
      return acc;
    }, {});
}

export function setClientCookie(name: string, value: string, opts?: { maxAgeSeconds?: number }) {
  const maxAge = opts?.maxAgeSeconds ?? 60 * 60 * 8; // 8h
  const secure = isProduction() ? '; Secure' : '';
  document.cookie = `${encodeURIComponent(name)}=${encodeURIComponent(value)}; Path=/; Max-Age=${maxAge}; SameSite=Lax${secure}`;
}

export function deleteClientCookie(name: string) {
  const secure = isProduction() ? '; Secure' : '';
  document.cookie = `${encodeURIComponent(name)}=; Path=/; Max-Age=0; SameSite=Lax${secure}`;
}
