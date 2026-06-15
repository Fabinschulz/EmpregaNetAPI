// https://nextjs.org/docs/app/api-reference/file-conventions/proxy

import { readSessionFromCookieHeader } from '@/services';
import { buildForbiddenRedirectPath, evaluateRouteAccess, LOGIN_PATH, type RouteAccessDecision } from '@/utils';
import { NextResponse, type NextRequest } from 'next/server';

const defaultOrigins = [
  'http://localhost:3000',
  'https://localhost:3000',
  'http://localhost:8081',
  'http://localhost:5225'
];

function getAllowedOrigins(): string[] {
  const fromEnv = process.env.NEXT_PUBLIC_ALLOWED_ORIGINS;
  if (!fromEnv?.trim()) return defaultOrigins;
  return fromEnv
    .split(',')
    .map((o) => o.trim())
    .filter(Boolean);
}

const corsOptions = {
  'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, PATCH, OPTIONS',
  'Access-Control-Allow-Headers': 'Content-Type, Authorization',
  'Access-Control-Allow-Credentials': 'true'
};

function routeAccessRedirect(req: NextRequest, pathname: string, decision: RouteAccessDecision): NextResponse | null {
  if (decision === 'allow') return null;

  const url = req.nextUrl.clone();
  url.search = '';

  if (decision === 'login') {
    url.pathname = LOGIN_PATH;
    url.searchParams.set('redirect', pathname);
    return NextResponse.redirect(url);
  }

  return NextResponse.redirect(new URL(buildForbiddenRedirectPath(pathname), req.url));
}

export function proxy(req: NextRequest) {
  const { pathname } = req.nextUrl;
  const allowedOrigins = getAllowedOrigins();
  const origin = req.headers.get('origin') ?? '';
  const isAllowedOrigin = allowedOrigins.includes(origin);
  const isPreflight = req.method === 'OPTIONS';

  if (isPreflight) {
    const preflightHeaders = {
      ...(isAllowedOrigin && { 'Access-Control-Allow-Origin': origin }),
      ...corsOptions
    };
    return NextResponse.json({}, { headers: preflightHeaders });
  }

  const session = readSessionFromCookieHeader(req.headers.get('cookie'));
  const accessDecision = evaluateRouteAccess(pathname, {
    token: session?.token ?? null,
    roles: session?.roles ?? []
  });
  const redirect = routeAccessRedirect(req, pathname, accessDecision);
  if (redirect) return redirect;

  const response = NextResponse.next();
  if (isAllowedOrigin) {
    response.headers.set('Access-Control-Allow-Origin', origin);
  }

  Object.entries(corsOptions).forEach(([key, value]) => {
    response.headers.set(key, value);
  });

  return response;
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)']
};
