'use client';

import * as React from 'react';
import { Button, type ButtonProps } from '@/components/ui';
import { useFormContext } from '@/context';

export type FormSubmitButtonProps = Omit<ButtonProps, 'type'>;

export const FormSubmitButton = React.forwardRef<HTMLButtonElement, FormSubmitButtonProps>(function FormSubmitButton(
  { disabled, children, ...props },
  ref
) {
  const { submitting } = useFormContext();
  return (
    <Button ref={ref} type="submit" disabled={!!disabled || submitting} {...props}>
      {children}
    </Button>
  );
});

FormSubmitButton.displayName = 'FormSubmitButton';
