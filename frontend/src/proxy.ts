// https://nextjs.org/docs/app/api-reference/file-conventions/proxy

import { ACCESS_TOKEN_COOKIE, isSessionValid, readSessionFromCookieHeader } from '@/shared/auth/session';
import { buildForbiddenRedirectPath, buildLoginRedirectPath, evaluateRouteAccess } from '@/shared/utils';
import { NextResponse, type NextRequest } from 'next/server';

export function proxy(req: NextRequest): NextResponse {
  const { pathname, search } = req.nextUrl;

  const session = readSessionFromCookieHeader(req.headers.get('cookie'));
  const decision = evaluateRouteAccess(pathname, {
    isAuthenticated: isSessionValid(session),
    roles: session?.roles ?? []
  });

  if (decision === 'login') {
    const response = NextResponse.redirect(new URL(buildLoginRedirectPath(`${pathname}${search}`), req.url));
    if (session?.token) response.cookies.delete(ACCESS_TOKEN_COOKIE);

    return response;
  }

  if (decision === 'forbidden') {
    return NextResponse.redirect(new URL(buildForbiddenRedirectPath(pathname), req.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)']
};
