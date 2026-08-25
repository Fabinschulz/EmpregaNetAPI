'use client';

import { Button, Spinner, type ButtonProps } from '@/shared/components';
import { useFormContext } from '@/shared/context';
import type { LucideIcon } from 'lucide-react';
import * as React from 'react';

export type FormSubmitButtonProps = Omit<ButtonProps, 'type' | 'startIcon'> & {
  icon?: LucideIcon;
};

export const FormSubmitButton = React.forwardRef<HTMLButtonElement, FormSubmitButtonProps>(function FormSubmitButton(
  { disabled, icon: Icon, children, ...props },
  ref
) {
  const { submitting } = useFormContext();

  return (
    <Button ref={ref} type="submit" disabled={!!disabled || submitting} aria-busy={submitting} {...props}>
      {submitting ? <Spinner size="sm" label={null} /> : Icon ? <Icon aria-hidden /> : null}
      {children}
    </Button>
  );
});

FormSubmitButton.displayName = 'FormSubmitButton';
