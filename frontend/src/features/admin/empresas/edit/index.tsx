'use client';

import { startTransition, useEffect, useMemo, useState, useTransition } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { FormFieldsSkeleton } from '@/components/common';
import { Alert, Button, FormSubmitButton, InputField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import { companyFormSchema, deleteCompany, getCompany, updateCompany, type CompanyFormValues } from '@/services';
import { useAuth } from '@/features/auth';
import { startRouterTransition } from '@/utils/lib';

function SaveCompanyButton() {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : 'Salvar'}</FormSubmitButton>;
}

export function AdminEditCompanyPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { token } = useAuth();

  const [pending, setPending] = useState(true);
  const [isMutating, startMutatingTransition] = useTransition();
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
        startTransition(() => {
          setInitial({
            name: res.name,
            email: res.email ?? '',
            phone: res.phone ?? '',
            documentNo: res.documentNo ?? ''
          });
        });
      } catch (err) {
        if (!mounted) return;
        startTransition(() => setApiError(err instanceof Error ? err.message : 'Erro ao carregar empresa.'));
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
        documentNo: data.documentNo.trim() || null
      });
      startRouterTransition(() => router.push('/admin/empresas'));
    } catch (err) {
      startTransition(() => setApiError(err instanceof Error ? err.message : 'Falha ao salvar.'));
    }
  }

  function onDelete() {
    if (!token) return;
    setApiError(null);
    startMutatingTransition(async () => {
      try {
        await deleteCompany(token, id);
        startRouterTransition(() => router.push('/admin/empresas'));
      } catch (err) {
        startTransition(() => setApiError(err instanceof Error ? err.message : 'Falha ao excluir.'));
      }
    });
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
    </div>
  );
}
