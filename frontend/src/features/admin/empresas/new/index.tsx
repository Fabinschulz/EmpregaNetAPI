"use client";

import { startTransition, useState } from "react";
import { useRouter } from "next/navigation";
import { Alert, FormSubmitButton, InputField } from "@/components";
import { FormProvider, useFormContext } from "@/context";
import { companyFormSchema, createCompany, type CompanyFormValues } from "@/services";
import { useAuth } from "@/features/auth";
import { startRouterTransition } from "@/utils/lib";

const companyEmpty: CompanyFormValues = {
  name: "",
  email: "",
  phone: "",
  documentNo: "",
};

function SaveCompanyButton({ label }: { label: string }) {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? "Salvando..." : label}</FormSubmitButton>;
}

export function AdminNewCompanyPage() {
  const router = useRouter();
  const { token } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);

  async function handleSubmit(data: CompanyFormValues) {
    if (!token) return;
    setApiError(null);
    try {
      await createCompany(token, {
        name: data.name,
        email: data.email.trim() || null,
        phone: data.phone.trim() || null,
        documentNo: data.documentNo.trim() || null,
      });
      startRouterTransition(() => router.push("/admin/empresas"));
    } catch (err) {
      startTransition(() =>
        setApiError(err instanceof Error ? err.message : "Falha ao criar empresa.")
      );
    }
  }

  return (
    <div>
      <h1>Nova empresa</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={companyFormSchema} defaultValues={companyEmpty} onSubmit={handleSubmit}>
        <div style={{ display: "grid", gap: 12, maxWidth: 640, marginTop: 12 }}>
          <InputField name="name" label="Nome" required />
          <InputField name="email" label="E-mail" type="email" />
          <InputField name="phone" label="Telefone" />
          <InputField name="documentNo" label="Documento" />
          <SaveCompanyButton label="Criar empresa" />
        </div>
      </FormProvider>
    </div>
  );
}

