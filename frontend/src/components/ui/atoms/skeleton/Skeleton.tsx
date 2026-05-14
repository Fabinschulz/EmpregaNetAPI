'use client';

import * as React from 'react';
import { cn } from '@/utils/lib';
import styles from './Skeleton.module.scss';

export type SkeletonProps = React.HTMLAttributes<HTMLDivElement>;

/**
 * Placeholder de carregamento (estilo shadcn, animação em SCSS).
 */
export function Skeleton({ className, style, ...props }: SkeletonProps) {
  return <div className={cn(styles.root, className)} style={style} {...props} />;
}
