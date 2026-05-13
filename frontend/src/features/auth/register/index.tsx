'use client';

import Link from 'next/link';
import { REGISTER_SECTION_STYLE } from './constants';
import { RegisterForm } from './register-form';

export function Register() {
  return (
    <section style={REGISTER_SECTION_STYLE}>
      <h1>Criar conta</h1>
      <p style={{ color: 'var(--muted)' }}>Crie sua conta e confirme o e-mail para iniciar sessão.</p>

      <RegisterForm />

      <p style={{ marginTop: 14, color: 'var(--muted)' }}>
        Já tem conta? <Link href="/login">Entrar</Link>
      </p>
    </section>
  );
}
