'use client';

import { Alert } from '@/components';
import { FormProvider } from '@/context';
import { companyFormSchema, defaultFormCompany, useCreateCompanyMutation, type CompanyFormValues } from '@/services';
import { CompanyFormFields } from '../form';

export function AdminNewCompanyPage() {
  const { apiError, mutateAsync } = useCreateCompanyMutation();
  const handleSubmit = async (formValue: CompanyFormValues) => await mutateAsync(formValue);

  return (
    <div>
      <h1>Nova empresa</h1>
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
