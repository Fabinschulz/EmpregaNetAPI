"use client";

import { startTransition, useEffect, useMemo, useState, useTransition } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, Button, FormSubmitButton, InputField, TextareaField } from "@/components";
import { FormProvider, useFormContext } from "@/context";
import { closeJob, getJob, jobFormSchema, updateJob, type JobFormValues } from "@/services";
import { useAuth } from "@/features/auth";
import { startRouterTransition } from "@/utils/lib";

function SaveJobButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? "Salvando..." : "Salvar"}</FormSubmitButton>;
}

export function RecruitmentEditJobPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const [pending, setPending] = useState(true);
  const [isMutating, startMutatingTransition] = useTransition();
  const [apiError, setApiError] = useState<string | null>(null);
  const [initial, setInitial] = useState<JobFormValues | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      setPending(true);
      setApiError(null);
      try {
        const res = await getJob(jobId);
        if (!mounted) return;
        startTransition(() => {
          setInitial({
            title: res.title,
            description: res.description ?? "",
            location: res.location ?? "",
          });
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() =>
          setApiError(err instanceof Error ? err.message : "Erro ao carregar vaga.")
        );
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [jobId]);

  async function handleSubmit(data: JobFormValues) {
    if (!token) return;
    setApiError(null);
    try {
      await updateJob(token, jobId, {
        title: data.title,
        description: data.description.trim() || null,
        location: data.location.trim() || null,
      });
      startRouterTransition(() => router.push("/recrutamento/vagas"));
    } catch (err) {
      startTransition(() =>
        setApiError(err instanceof Error ? err.message : "Falha ao salvar.")
      );
    }
  }

  function onClose() {
    if (!token) return;
    setApiError(null);
    startMutatingTransition(async () => {
      try {
        await closeJob(token, jobId);
        startRouterTransition(() => router.push("/recrutamento/vagas"));
      } catch (err) {
        startTransition(() =>
          setApiError(err instanceof Error ? err.message : "Falha ao encerrar.")
        );
      }
    });
  }

  return (
    <div>
      <h1>Editar vaga</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}
      {pending ? (
        <p>Carregando...</p>
      ) : initial ? (
        <FormProvider
          key={`job-${jobId}`}
          validationSchema={jobFormSchema}
          defaultValues={initial}
          onSubmit={handleSubmit}
        >
          <div style={{ display: "grid", gap: 12, maxWidth: 640, marginTop: 12 }}>
            <InputField name="title" label="Título" required />
            <InputField name="location" label="Localização" />
            <TextareaField name="description" label="Descrição" rows={5} />
            <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
              <SaveJobButton />
              <Button type="button" onClick={onClose} disabled={isMutating}>
                Encerrar vaga
              </Button>
            </div>
          </div>
        </FormProvider>
      ) : null}
    </div>
  );
}

