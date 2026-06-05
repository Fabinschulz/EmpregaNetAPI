// https://nextjs.org/docs/app/api-reference/file-conventions/proxy

import { readSessionFromCookieHeader } from '@/services';
import { canAccessPath, isPublicPath } from '@/utils';
import { NextResponse, type NextRequest } from 'next/server';

const defaultOrigins = ['http://localhost:3000', 'https://localhost:3000', 'http://localhost:8081', 'http://localhost:5225'];

function getAllowedOrigins(): string[] {
  const fromEnv = process.env.NEXT_PUBLIC_ALLOWED_ORIGINS;
  if (!fromEnv?.trim()) return defaultOrigins;
  return fromEnv.split(',').map((o) => o.trim()).filter(Boolean);
}

const corsOptions = {
  'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, PATCH, OPTIONS',
  'Access-Control-Allow-Headers': 'Content-Type, Authorization',
  'Access-Control-Allow-Credentials': 'true'
};

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

  if (!isPublicPath(pathname)) {
    const session = readSessionFromCookieHeader(req.headers.get('cookie'));
    if (!session?.token) {
      const url = req.nextUrl.clone();
      url.pathname = '/login';
      url.searchParams.set('next', pathname);
      return NextResponse.redirect(url);
    }

    if (!canAccessPath(pathname, session.roles)) {
      const url = req.nextUrl.clone();
      url.pathname = '/dashboard';
      return NextResponse.redirect(url);
    }
  }

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
