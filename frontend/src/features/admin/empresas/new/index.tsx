'use client';

import { Alert, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import { companyFormSchema, defaultFormCompany, useCreateCompanyMutation, type CompanyFormValues } from '../service';
import { CompanyFormFields } from '../form';

export function AdminNewCompanyPage() {
  const { apiError, mutateAsync } = useCreateCompanyMutation();
  const handleSubmit = async (formValue: CompanyFormValues) => await mutateAsync(formValue);

  return (
    <div>
      <PageHeader title="Nova empresa" description="Cadastre uma nova empresa parceira." />
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={companyFormSchema} defaultValues={defaultFormCompany} onSubmit={handleSubmit}>
        <CompanyFormFields submitLabel="Criar empresa" />
      </FormProvider>
    </div>
  );
}
