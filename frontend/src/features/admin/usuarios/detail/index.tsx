'use client';

import { useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Alert, ApiQueryBoundary, Button, FormFieldsSkeleton, FormSubmitButton, InputField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import {
  adminUserUpdateFormSchema,
  useAdminUserQuery,
  useDeleteAdminUserMutation,
  useUpdateAdminUserMutation,
  type AdminUserUpdateFormValues
} from '@/services';
import { getApiErrorMessage } from '@/utils/helpers';
import { startRouterTransition } from '@/utils/lib';

function SaveUserButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Salvar'}</FormSubmitButton>;
}

export function AdminUserDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: user, isPending, isError, error, refetch } = useAdminUserQuery(id);
  const updateMutation = useUpdateAdminUserMutation();
  const deleteMutation = useDeleteAdminUserMutation();
  const [apiError, setApiError] = useState<string | null>(null);

  const initial = useMemo<AdminUserUpdateFormValues | null>(() => {
    if (!user) return null;
    return { userType: (user.userType ?? '') as string };
  }, [user]);

  async function handleSubmit(data: AdminUserUpdateFormValues) {
    setApiError(null);
    updateMutation.mutate(
      { id, dto: { userType: data.userType.trim() || null } },
      {
        onSuccess: () => startRouterTransition(() => router.push('/admin/usuarios')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'usuário'))
      }
    );
  }

  function onDelete() {
    setApiError(null);
    deleteMutation.mutate(
      { id },
      {
        onSuccess: () => startRouterTransition(() => router.push('/admin/usuarios')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'usuário'))
      }
    );
  }

  const isMutating = updateMutation.isPending || deleteMutation.isPending;

  return (
    <ApiQueryBoundary
      fallback="usuário"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="usuário"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Admin: Usuário</h1>
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? <FormFieldsSkeleton fields={6} /> : null}
        {user ? (
          <>
            <article
              style={{
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius)',
                padding: 14,
                background: 'var(--card-bg)'
              }}
            >
              <p>
                <strong>ID:</strong> {user.id}
              </p>
              <p>
                <strong>Usuário:</strong> {user.username}
              </p>
              <p>
                <strong>E-mail:</strong> {user.email}
              </p>
            </article>

            {initial ? (
              <FormProvider
                key={`admin-user-${id}`}
                validationSchema={adminUserUpdateFormSchema}
                defaultValues={initial}
                onSubmit={handleSubmit}
              >
                <div style={{ display: 'grid', gap: 12, maxWidth: 520, marginTop: 12 }}>
                  <InputField
                    name="userType"
                    label="UserType (ex.: Admin, Recruiter, Manager, Candidate)"
                    hint="O backend valida/normaliza; aqui enviamos o valor diretamente."
                  />
                  <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
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
      </section>
    </ApiQueryBoundary>
  );
}
