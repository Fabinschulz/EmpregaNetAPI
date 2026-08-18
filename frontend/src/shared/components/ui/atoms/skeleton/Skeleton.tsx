'use client';

import { cn } from '@/shared/utils';
import * as React from 'react';
import styles from './Skeleton.module.scss';

export type SkeletonProps = React.HTMLAttributes<HTMLDivElement>;

/**
 * Placeholder de carregamento (estilo shadcn, animação em SCSS).
 */
export function Skeleton({ className, style, ...props }: SkeletonProps) {
  return <div className={cn(styles.root, className)} style={style} {...props} />;
}
