'use client';

import * as React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/utils/lib';
import styles from './Badge.module.scss';

const badgeVariants = cva(styles.root, {
  variants: {
    variant: {
      default: styles.variantDefault,
      secondary: styles.variantSecondary,
      destructive: styles.variantDestructive,
      success: styles.variantSuccess,
      warning: styles.variantWarning
    }
  },
  defaultVariants: {
    variant: 'default'
  }
});

export type BadgeProps = React.HTMLAttributes<HTMLSpanElement> &
  VariantProps<typeof badgeVariants> & {
    className?: string;
  };

function Badge({ className, variant, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export { Badge, badgeVariants };
