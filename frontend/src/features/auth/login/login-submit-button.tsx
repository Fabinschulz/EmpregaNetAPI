'use client';

import { FormSubmitButton } from '@/components';
import { useFormContext } from '@/context';

export function LoginSubmitButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Entrando...' : 'Entrar'}</FormSubmitButton>;
}
