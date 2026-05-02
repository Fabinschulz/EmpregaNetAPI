"use client";

import type { ReactNode } from "react";
import { AuthLayoutFrame } from "./auth-layout-frame";
import { AuthSessionBoundary } from "./auth-session-boundary";

type AuthLayoutProps = {
  children: ReactNode;
};

export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <AuthLayoutFrame>
      <AuthSessionBoundary>{children}</AuthSessionBoundary>
    </AuthLayoutFrame>
  );
}
