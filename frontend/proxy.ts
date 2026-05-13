import { NextResponse, type NextRequest } from 'next/server';
import { readSessionFromCookieHeader } from '@/features/auth/session';
import { canAccessPath } from '@/utils/lib';

const PUBLIC_PATHS = ['/login', '/register'];

export function proxy(req: NextRequest) {
  const { pathname } = req.nextUrl;

  if (pathname.startsWith('/_next') || pathname.startsWith('/favicon') || pathname.startsWith('/assets')) {
    return NextResponse.next();
  }

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

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api).*)']
};
