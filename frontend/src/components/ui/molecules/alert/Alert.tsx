'use client';

import * as React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/utils/lib';
import styles from './Alert.module.scss';

const alertVariants = cva(styles.alert, {
  variants: {
    variant: {
      default: '',
      destructive: styles.destructive,
      success: styles.success
    }
  },
  defaultVariants: {
    variant: 'default'
  }
});

export type AlertProps = React.HTMLAttributes<HTMLDivElement> &
  VariantProps<typeof alertVariants> & {
    title?: string;
  };

function Alert({ className, variant, title, children, ...props }: AlertProps) {
  return (
    <div
      role={variant === 'destructive' ? 'alert' : 'status'}
      className={cn(alertVariants({ variant }), className)}
      {...props}
    >
      {title ? <p className={styles.title}>{title}</p> : null}
      <p className={styles.body}>{children}</p>
    </div>
  );
}

export { Alert, alertVariants };
