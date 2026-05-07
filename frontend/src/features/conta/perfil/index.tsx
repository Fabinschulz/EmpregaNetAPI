"use client";

import { startTransition, useEffect, useState } from "react";
import { Alert } from "@/components/ui";
import { me } from "@/services";
import { useAuth } from "@/features/auth";

export function ProfilePage() {
  const { token, roles } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [user, setUser] = useState<{ id: number; username: string; email: string } | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setError(null);
      try {
        const res = await me(token);
        if (!mounted) return;
        startTransition(() => {
          setUser({ id: res.id, username: res.username, email: res.email });
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() =>
          setError(err instanceof Error ? err.message : "Erro ao carregar perfil.")
        );
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [token]);

  return (
    <div>
      <h1>Minha conta</h1>
      <p style={{ color: "var(--muted)" }}>Informações do usuário autenticado.</p>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}

      {user ? (
        <div
          style={{
            border: "1px solid var(--border)",
            borderRadius: "var(--radius)",
            padding: 14,
            background: "rgba(255,255,255,0.05)",
          }}
        >
          <div>
            <strong>ID:</strong> {user.id}
          </div>
          <div>
            <strong>Usuário:</strong> {user.username}
          </div>
          <div>
            <strong>E-mail:</strong> {user.email}
          </div>
          <div>
            <strong>Roles:</strong> {roles.length ? roles.join(", ") : "—"}
          </div>
        </div>
      ) : null}
    </div>
  );
}

