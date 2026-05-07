"use client";

import { startTransition, useEffect, useMemo, useState } from "react";
import { useParams } from "next/navigation";
import { Alert } from "@/components/ui";
import { getCandidate } from "@/services";
import { useAuth } from "@/features/auth";

export function CandidateDetailPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

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
        const res = await getCandidate(token, id);
        if (!mounted) return;
        startTransition(() => {
          setUser({ id: res.id, username: res.username, email: res.email });
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() =>
          setError(err instanceof Error ? err.message : "Erro ao carregar candidato.")
        );
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [token, id]);

  return (
    <div>
      <h1>Candidato</h1>
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
        </div>
      ) : null}
    </div>
  );
}

