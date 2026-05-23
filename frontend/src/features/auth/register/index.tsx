'use client';

import { Alert } from '@/components';
import { FormProvider } from '@/context';
import { useRegisterMutation } from '@/services';
import Link from 'next/link';
import { CSSProperties } from 'react';
import { RegisterFormFields } from './register-form';
import type { RegisterDto } from './register-schema';
import { registerDefaultValues, registerFormSchema } from './register-schema';

const REGISTER_SECTION_STYLE: CSSProperties = {
  maxWidth: 520,
  margin: '0 auto'
};

export function Register() {
  const { apiError, mutateAsync } = useRegisterMutation();
  const handleSubmit = async (formValue: RegisterDto) => await mutateAsync(formValue);

  return (
    <section style={REGISTER_SECTION_STYLE}>
      <h1>Criar conta</h1>
      <p style={{ color: 'var(--muted)' }}>Crie sua conta e confirme o e-mail para iniciar sessão.</p>

      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <RegisterFormFields />
      </FormProvider>

      <p style={{ marginTop: 14, color: 'var(--muted)' }}>
        Já tem conta? <Link href="/login">Entrar</Link>
      </p>
    </section>
  );
}
