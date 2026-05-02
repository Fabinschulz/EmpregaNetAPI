"use client";

import { useEffect, useMemo, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, Button, FormSubmitButton, InputField } from "@/components";
import { FormProvider, useFormContext } from "@/context";
import { companyFormSchema, deleteCompany, getCompany, updateCompany, type CompanyFormValues } from "@/services";
import { useAuth } from "@/features/auth";

function SaveCompanyButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? "Salvando..." : "Salvar"}</FormSubmitButton>;
}

export function AdminEditCompanyPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const [pending, setPending] = useState(true);
  const [saving, setSaving] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [initial, setInitial] = useState<CompanyFormValues | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setApiError(null);
      try {
        const res = await getCompany(token, id);
        if (!mounted) return;
        setInitial({
          name: res.name,
          email: res.email ?? "",
          phone: res.phone ?? "",
          documentNo: res.documentNo ?? "",
        });
      } catch (err) {
        if (!mounted) return;
        setApiError(err instanceof Error ? err.message : "Erro ao carregar empresa.");
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [token, id]);

  async function handleSubmit(data: CompanyFormValues) {
    if (!token) return;
    setApiError(null);
    try {
      await updateCompany(token, id, {
        name: data.name,
        email: data.email.trim() || null,
        phone: data.phone.trim() || null,
        documentNo: data.documentNo.trim() || null,
      });
      router.push("/admin/empresas");
    } catch (err) {
      setApiError(err instanceof Error ? err.message : "Falha ao salvar.");
    }
  }

  async function onDelete() {
    if (!token) return;
    setSaving(true);
    setApiError(null);
    try {
      await deleteCompany(token, id);
      router.push("/admin/empresas");
    } catch (err) {
      setApiError(err instanceof Error ? err.message : "Falha ao excluir.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <h1>Editar empresa</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}
      {pending ? (
        <p>Carregando...</p>
      ) : initial ? (
        <FormProvider
          key={`company-${id}`}
          validationSchema={companyFormSchema}
          defaultValues={initial}
          onSubmit={handleSubmit}
        >
          <div style={{ display: "grid", gap: 12, maxWidth: 640, marginTop: 12 }}>
            <InputField name="name" label="Nome" required />
            <InputField name="email" label="E-mail" type="email" />
            <InputField name="phone" label="Telefone" />
            <InputField name="documentNo" label="Documento" />
            <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
              <SaveCompanyButton />
              <Button variant="destructive" type="button" onClick={onDelete} disabled={saving}>
                Excluir
              </Button>
            </div>
          </div>
        </FormProvider>
      ) : null}
    </div>
  );
}

