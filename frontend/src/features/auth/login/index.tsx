'use client';

import { Alert } from '@/components';
import { FormProvider } from '@/context';
import { useLoginMutation } from '@/services';
import Link from 'next/link';
import { CSSProperties } from 'react';
import { LoginFormFields } from './login-form';
import type { LoginDto } from './login-schema';
import { loginDefaultValues, loginSchema } from './login-schema';

const LOGIN_SECTION_STYLE: CSSProperties = {
  maxWidth: 520,
  margin: '0 auto'
};

export function Login() {
  const { apiError, mutateAsync } = useLoginMutation();
  const handleSubmit = async (formValue: LoginDto) => await mutateAsync(formValue);

  return (
    <section style={LOGIN_SECTION_STYLE}>
      <h1>Entrar</h1>
      <p style={{ color: 'var(--muted)' }}>Use seu e-mail e senha para iniciar sessão.</p>

      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <LoginFormFields />
      </FormProvider>

      <p style={{ marginTop: 14, color: 'var(--muted)' }}>
        Não tem conta? <Link href="/register">Criar conta</Link>
      </p>
    </section>
  );
}
