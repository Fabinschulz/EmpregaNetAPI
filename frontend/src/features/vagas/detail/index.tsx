"use client";

import { useEffect, useMemo, useState } from "react";
import { useParams } from "next/navigation";
import { Alert, Button } from "@/components/ui";
import { applyToJob, getJob } from "@/services";
import { useAuth } from "@/features/auth";

export function JobDetailPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [job, setJob] = useState<{ id: number; title: string; description?: string | null } | null>(null);
  const [appPending, setAppPending] = useState(false);
  const [appMsg, setAppMsg] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      setPending(true);
      setError(null);
      try {
        const res = await getJob(jobId);
        if (!mounted) return;
        setJob(res);
      } catch (err) {
        if (!mounted) return;
        setError(err instanceof Error ? err.message : "Erro ao carregar vaga.");
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [jobId]);

  async function onApply() {
    if (!token) {
      setAppMsg("Você precisa estar logado para se candidatar.");
      return;
    }
    setAppPending(true);
    setAppMsg(null);
    try {
      const res = await applyToJob(token, { jobId });
      setAppMsg(typeof res === "string" ? res : "Candidatura enviada.");
    } catch (err) {
      setAppMsg(err instanceof Error ? err.message : "Falha ao se candidatar.");
    } finally {
      setAppPending(false);
    }
  }

  return (
    <div>
      {pending ? <p>Carregando...</p> : null}
      {error ? (
        <Alert variant="destructive" title="Erro">
          {error}
        </Alert>
      ) : null}
      {job ? (
        <>
          <h1>{job.title}</h1>
          <p style={{ color: "var(--muted)" }}>{job.description ?? "Sem descrição."}</p>

          <div style={{ marginTop: 14, display: "flex", gap: 10, flexWrap: "wrap" }}>
            <Button variant="primary" onClick={onApply} disabled={appPending}>
              {appPending ? "Enviando..." : "Candidatar-me"}
            </Button>
          </div>
          {appMsg ? (
            <div style={{ marginTop: 12 }}>
              <Alert title="Status">{appMsg}</Alert>
            </div>
          ) : null}
        </>
      ) : null}
    </div>
  );
}

