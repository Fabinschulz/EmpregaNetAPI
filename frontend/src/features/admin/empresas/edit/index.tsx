'use client';

import { useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Alert, ApiQueryBoundary, Button, FormFieldsSkeleton, FormSubmitButton, InputField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import {
  companyFormSchema,
  useCompanyQuery,
  useDeleteCompanyMutation,
  useUpdateCompanyMutation,
  type CompanyFormValues
} from '@/services';
import { getApiErrorMessage, startRouterTransition } from '@/utils';

function SaveCompanyButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Salvar'}</FormSubmitButton>;
}

export function AdminEditCompanyPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: company, isPending, isError, error, refetch } = useCompanyQuery(id);
  const updateMutation = useUpdateCompanyMutation();
  const deleteMutation = useDeleteCompanyMutation();
  const [apiError, setApiError] = useState<string | null>(null);

  const initial = useMemo<CompanyFormValues | null>(() => {
    if (!company) return null;
    return {
      name: company.name,
      email: company.email ?? '',
      phone: company.phone ?? '',
      documentNo: company.documentNo ?? ''
    };
  }, [company]);

  async function handleSubmit(data: CompanyFormValues) {
    setApiError(null);
    updateMutation.mutate(
      {
        id,
        dto: {
          name: data.name,
          email: data.email.trim() || null,
          phone: data.phone.trim() || null,
          documentNo: data.documentNo.trim() || null
        }
      },
      {
        onSuccess: () => startRouterTransition(() => router.push('/admin/empresas')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'empresa'))
      }
    );
  }

  function onDelete() {
    setApiError(null);
    deleteMutation.mutate(
      { id },
      {
        onSuccess: () => startRouterTransition(() => router.push('/admin/empresas')),
        onError: (err) => setApiError(getApiErrorMessage(err, 'empresa'))
      }
    );
  }

  const isMutating = updateMutation.isPending || deleteMutation.isPending;

  return (
    <ApiQueryBoundary
      fallback="empresa"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="empresa"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Editar empresa</h1>
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? (
          <FormFieldsSkeleton fields={8} />
        ) : initial ? (
          <FormProvider
            key={`company-${id}`}
            validationSchema={companyFormSchema}
            defaultValues={initial}
            onSubmit={handleSubmit}
          >
            <div style={{ display: 'grid', gap: 12, maxWidth: 640, marginTop: 12 }}>
              <InputField name="name" label="Nome" required />
              <InputField name="email" label="E-mail" type="email" />
              <InputField name="phone" label="Telefone" />
              <InputField name="documentNo" label="Documento" />
              <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                <SaveCompanyButton />
                <Button variant="destructive" type="button" onClick={onDelete} disabled={isMutating}>
                  Excluir
                </Button>
              </div>
            </div>
          </FormProvider>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
