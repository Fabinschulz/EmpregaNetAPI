import type { ReactNode } from "react";

type AuthLayoutFrameProps = {
  children: ReactNode;
};

export function AuthLayoutFrame({ children }: AuthLayoutFrameProps) {
  return <main className="container">{children}</main>;
}
