'use client';

import { Alert, FormSubmitButton, InputField } from '@/components';
import { FormProvider, useFormContext } from '@/context';
import { companyFormSchema, useCreateCompanyMutation, type CompanyFormValues } from '@/services';
import { getApiErrorMessage, startRouterTransition } from '@/utils';
import { useRouter } from 'next/navigation';
import { useState } from 'react';

const companyEmpty: CompanyFormValues = {
  name: '',
  email: '',
  phone: '',
  documentNo: ''
};

function SaveCompanyButton({ label }: { label: string }) {
  const { submitting } = useFormContext();
  return <FormSubmitButton variant="primary">{submitting ? 'Salvando...' : label}</FormSubmitButton>;
}

export function AdminNewCompanyPage() {
  const router = useRouter();
  const createMutation = useCreateCompanyMutation();
  const [apiError, setApiError] = useState<string | null>(null);

  async function handleSubmit(data: CompanyFormValues) {
    setApiError(null);
    createMutation.mutate(
      {
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

  return (
    <div>
      <h1>Nova empresa</h1>
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={companyFormSchema} defaultValues={companyEmpty} onSubmit={handleSubmit}>
        <div style={{ display: 'grid', gap: 12, maxWidth: 640, marginTop: 12 }}>
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
