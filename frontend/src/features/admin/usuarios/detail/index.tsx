"use client";

import { startTransition, useEffect, useMemo, useState, useTransition } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, Button, FormSubmitButton, InputField } from "@/components";
import { FormProvider, useFormContext } from "@/context";
import {
  adminUserUpdateFormSchema,
  deleteAdminUser,
  getAdminUser,
  updateAdminUser,
  type AdminUserUpdateFormValues,
} from "@/services";
import { useAuth } from "@/features/auth";
import { startRouterTransition } from "@/utils/lib";

function SaveUserButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? "Salvando..." : "Salvar"}</FormSubmitButton>;
}

export function AdminUserDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const [pending, setPending] = useState(true);
  const [isMutating, startMutatingTransition] = useTransition();
  const [apiError, setApiError] = useState<string | null>(null);
  const [user, setUser] = useState<{ id: number; username: string; email: string } | null>(null);
  const [initial, setInitial] = useState<AdminUserUpdateFormValues | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      if (!token) return;
      setPending(true);
      setApiError(null);
      try {
        const res = await getAdminUser(token, id);
        if (!mounted) return;
        startTransition(() => {
          setUser({ id: res.id, username: res.username, email: res.email });
          setInitial({ userType: (res.userType ?? "") as string });
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() =>
          setApiError(err instanceof Error ? err.message : "Erro ao carregar usuário.")
        );
      } finally {
        if (mounted) setPending(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [token, id]);

  async function handleSubmit(data: AdminUserUpdateFormValues) {
    if (!token) return;
    setApiError(null);
    try {
      await updateAdminUser(token, id, { userType: data.userType.trim() || null });
      startRouterTransition(() => router.push("/admin/usuarios"));
    } catch (err) {
      startTransition(() =>
        setApiError(err instanceof Error ? err.message : "Falha ao salvar.")
      );
    }
  }

  function onDelete() {
    if (!token) return;
    setApiError(null);
    startMutatingTransition(async () => {
      try {
        await deleteAdminUser(token, id);
        startRouterTransition(() => router.push("/admin/usuarios"));
      } catch (err) {
        startTransition(() =>
          setApiError(err instanceof Error ? err.message : "Falha ao excluir.")
        );
      }
    });
  }

  return (
    <div>
      <h1>Admin: Usuário</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}
      {pending ? <p>Carregando...</p> : null}
      {user ? (
        <>
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

          {initial ? (
            <FormProvider
              key={`admin-user-${id}`}
              validationSchema={adminUserUpdateFormSchema}
              defaultValues={initial}
              onSubmit={handleSubmit}
            >
              <div style={{ display: "grid", gap: 12, maxWidth: 520, marginTop: 12 }}>
                <InputField
                  name="userType"
                  label="UserType (ex.: Admin, Recruiter, Manager, Candidate)"
                  hint="O backend valida/normaliza; aqui enviamos o valor diretamente."
                />
                <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                  <SaveUserButton />
                  <Button variant="destructive" type="button" onClick={onDelete} disabled={isMutating}>
                    Excluir (lógico)
                  </Button>
                </div>
              </div>
            </FormProvider>
          ) : null}
        </>
      ) : null}
    </div>
  );
}

