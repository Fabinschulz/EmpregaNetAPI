'use client';

import { Alert, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import { CompanyFormFields, companyFormSchema, defaultFormCompany, type CompanyFormValues } from '../form';
import { companiesRoutes } from '../companies-routes';
import { useCreateCompanyMutation } from '../service';

export function AdminNewCompanyPage() {
  const { apiError, mutateAsync } = useCreateCompanyMutation();
  const handleSubmit = async (formValue: CompanyFormValues) => await mutateAsync(formValue);

  return (
    <div>
      <PageHeader title="Nova empresa" description="Cadastre uma nova empresa parceira." />
      {apiError ? (
        <Alert variant="destructive" title="Erro" style={{ margin: '1rem 0' }}>
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={companyFormSchema} defaultValues={defaultFormCompany} onSubmit={handleSubmit}>
        <CompanyFormFields submitLabel="Criar empresa" backHref={companiesRoutes.list} />
      </FormProvider>
    </div>
  );
}
