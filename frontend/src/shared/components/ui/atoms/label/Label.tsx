'use client';

import { cn } from '@/shared/utils/lib';
import * as LabelPrimitive from '@radix-ui/react-label';
import * as React from 'react';
import styles from './Label.module.scss';

const Label = React.forwardRef<
  React.ComponentRef<typeof LabelPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof LabelPrimitive.Root>
>(({ className, ...props }, ref) => (
  <LabelPrimitive.Root ref={ref} className={cn(styles.root, className)} {...props} />
));
Label.displayName = LabelPrimitive.Root.displayName;

export { Label };
