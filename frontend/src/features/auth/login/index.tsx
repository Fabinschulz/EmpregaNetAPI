"use client";

import Link from "next/link";
import { LOGIN_SECTION_STYLE } from "./constants";
import { LoginForm } from "./login-form";

export function Login() {
  return (
    <section style={LOGIN_SECTION_STYLE}>
      <h1>Entrar</h1>
      <p style={{ color: "var(--muted)" }}>Use seu e-mail e senha para iniciar sessão.</p>

      <LoginForm />

      <p style={{ marginTop: 14, color: "var(--muted)" }}>
        Não tem conta? <Link href="/register">Criar conta</Link>
      </p>
    </section>
  );
}
