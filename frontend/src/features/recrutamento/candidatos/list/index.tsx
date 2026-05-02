"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Alert, Button } from "@/components/ui";
import { listCandidates } from "@/services";
import { useAuth } from "@/features/auth";

export function RecruitmentCandidatesPage() {
  const { token } = useAuth();
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<{ id: number; username: string; email: string }[]>([]);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setError(null);
      try {
        const res = await listCandidates(token, { page: 1, size: 100 });
        if (!mounted) return;
        setItems(res.data.map((u) => ({ id: u.id, username: u.username, email: u.email })));
      } catch (err) {
        if (!mounted) return;
        setError(err instanceof Error ? err.message : "Erro ao carregar candidatos.");
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
      <h1>Recrutamento: Candidatos</h1>
      <p style={{ color: "var(--muted)" }}>Consulta de candidatos (recrutamento).</p>

      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}

      {!pending && items.length === 0 ? (
        <Alert title="Nenhum candidato">Nenhum candidato encontrado.</Alert>
      ) : (
        <div style={{ display: "grid", gap: 10, marginTop: 12 }}>
          {items.map((u) => (
            <div
              key={u.id}
              style={{
                border: "1px solid var(--border)",
                borderRadius: "var(--radius)",
                padding: 14,
                background: "rgba(255,255,255,0.05)",
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: 12,
              }}
            >
              <div>
                <div style={{ fontWeight: 700 }}>{u.username}</div>
                <div style={{ color: "var(--muted)", fontSize: 14 }}>{u.email}</div>
              </div>
              <Button asChild>
                <Link href={`/recrutamento/candidatos/${u.id}`}>Ver</Link>
              </Button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

