'use client';

import { Alert, ApiQueryBoundary, FormFieldsSkeleton, FormHeader, FormNotice, PageHeader } from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { companiesRoutes } from '../companies-routes';
import {
  CompanyFormFields,
  companyFormSchema,
  companyFormValuesFromResponse,
  defaultFormCompany,
  type CompanyFormValues
} from '../form';
import { useCompanyQuery, useUpdateCompanyMutation } from '../service';

const HEADING = {
  title: 'Editar empresa',
  description: 'Atualize os dados cadastrais da empresa.'
} as const;

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
      {isPending ? (
        <section>
          <PageHeader {...HEADING} />
          <FormFieldsSkeleton fields={8} />
        </section>
      ) : (
        <FormProvider
          key={`company-${id}`}
          validationSchema={companyFormSchema}
          defaultValues={initial}
          onSubmit={handleSubmit}
        >
          <FormHeader {...HEADING} backHref={companiesRoutes.list} submitLabel="Salvar" />
          {apiError ? (
            <FormNotice>
              <Alert variant="destructive" title="Erro">
                {apiError}
              </Alert>
            </FormNotice>
          ) : null}
          <CompanyFormFields />
        </FormProvider>
      )}
    </ApiQueryBoundary>
  );
}
