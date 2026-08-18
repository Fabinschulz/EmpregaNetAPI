'use client';

import { Button, Spinner, type ButtonProps } from '@/shared/components';
import { useFormContext } from '@/shared/context';
import * as React from 'react';

export type FormSubmitButtonProps = Omit<ButtonProps, 'type'>;

export const FormSubmitButton = React.forwardRef<HTMLButtonElement, FormSubmitButtonProps>(function FormSubmitButton(
  { disabled, children, ...props },
  ref
) {
  const { submitting } = useFormContext();

  return (
    <Button ref={ref} type="submit" disabled={!!disabled || submitting} aria-busy={submitting} {...props}>
      {submitting ? <Spinner size="sm" label={null} /> : null}
      {children}
    </Button>
  );
});

FormSubmitButton.displayName = 'FormSubmitButton';
