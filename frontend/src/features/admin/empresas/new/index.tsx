'use client';

import { Alert, FormHeader, FormNotice } from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { CompanyFormFields, companyFormSchema, defaultFormCompany, type CompanyFormValues } from '../form';
import { companiesRoutes } from '../companies-routes';
import { useCreateCompanyMutation } from '../service';

export function AdminNewCompanyPage() {
  const { apiError, mutateAsync } = useCreateCompanyMutation();
  const handleSubmit = async (formValue: CompanyFormValues) => await mutateAsync(formValue);

  return (
    <FormProvider validationSchema={companyFormSchema} defaultValues={defaultFormCompany} onSubmit={handleSubmit}>
      <FormHeader
        title="Nova empresa"
        description="Cadastre uma nova empresa parceira."
        backHref={companiesRoutes.list}
        submitLabel="Criar empresa"
      />
      {apiError ? (
        <FormNotice>
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        </FormNotice>
      ) : null}
      <CompanyFormFields />
    </FormProvider>
  );
}
