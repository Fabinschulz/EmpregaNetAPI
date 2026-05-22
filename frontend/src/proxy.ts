// https://nextjs.org/docs/app/api-reference/file-conventions/proxy

import { canAccessPath } from '@/utils/lib';
import { NextResponse, type NextRequest } from 'next/server';
import { readSessionFromCookieHeader } from 'src/services';

const PUBLIC_PATHS = ['/login', '/register'];
const allowedOrigins = ['http://localhost:8081', 'http://localhost:3000'];

const corsOptions = {
  'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
  'Access-Control-Allow-Headers': 'Content-Type, Authorization'
};

export function proxy(req: NextRequest) {
  const { pathname } = req.nextUrl;
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

  // if (pathname.startsWith('/_next') || pathname.startsWith('/favicon') || pathname.startsWith('/assets')) {
  //   return NextResponse.next();
  // }

  if (PUBLIC_PATHS.some((p) => pathname === p || pathname.startsWith(p + '/'))) {
    return NextResponse.next();
  }

  const session = readSessionFromCookieHeader(req.headers.get('cookie'));
  if (!session?.token) {
    const url = req.nextUrl.clone();
    url.pathname = '/login';
    return NextResponse.redirect(url);
  }

  if (!canAccessPath(pathname, session.roles)) {
    const url = req.nextUrl.clone();
    url.pathname = '/dashboard';
    return NextResponse.redirect(url);
  }

  // Handle simple requests
  const response = NextResponse.next()
  if (isAllowedOrigin) {
    req.headers.set('Access-Control-Allow-Origin', origin);
  }

  Object.entries(corsOptions).forEach(([key, value]) => {
    req.headers.set(key, value)
  });

  return response;
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)']
};
