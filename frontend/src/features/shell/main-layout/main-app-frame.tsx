'use client';

import type { ReactNode } from 'react';
import { AppShell } from '../AppShell';

type MainAppFrameProps = {
  children: ReactNode;
};

/** Chrome da app (sidebar, topbar) + área de conteúdo. */
export function MainAppFrame({ children }: MainAppFrameProps) {
  return <AppShell>{children}</AppShell>;
}
