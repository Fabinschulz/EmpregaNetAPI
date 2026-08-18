'use client';

import { useMemo } from 'react';
import { useParams } from 'next/navigation';
import { Alert, ApiQueryBoundary, FormFieldsSkeleton, PageHeader } from '@/shared/components';
import { FormProvider } from '@/shared/context';
import {
  CompanyFormFields,
  companyFormSchema,
  companyFormValuesFromResponse,
  defaultFormCompany,
  type CompanyFormValues
} from '../form';
import { companiesRoutes } from '../companies-routes';
import { useCompanyQuery, useUpdateCompanyMutation } from '../service';

export function AdminEditCompanyPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: company, isPending, isError, error, refetch } = useCompanyQuery(id);
  const { apiError, mutateAsync } = useUpdateCompanyMutation(id);

  const initial = useMemo<CompanyFormValues>(() => {
    if (!company) return defaultFormCompany;
    return companyFormValuesFromResponse(company);
  }, [company]);

  const handleSubmit = async (formValue: CompanyFormValues) => await mutateAsync(formValue);

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
        <PageHeader title="Editar empresa" description="Atualize os dados cadastrais da empresa." />
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? (
          <FormFieldsSkeleton fields={8} />
        ) : (
          <FormProvider
            key={`company-${id}`}
            validationSchema={companyFormSchema}
            defaultValues={initial}
            onSubmit={handleSubmit}
          >
            <CompanyFormFields submitLabel="Salvar" backHref={companiesRoutes.list} />
          </FormProvider>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
