'use client';

import { Button, type ButtonProps } from '@/components';
import { useFormContext } from '@/context';
import * as React from 'react';

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
