'use client';

import { useMemo } from 'react';
import { useParams } from 'next/navigation';
import { Alert, ApiQueryBoundary, FormFieldsSkeleton, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import {
  companyFormSchema,
  companyFormValuesFromDto,
  defaultFormCompany,
  useCompanyQuery,
  useUpdateCompanyMutation,
  type CompanyFormValues
} from '../service';
import { CompanyFormFields } from '../form';

export function AdminEditCompanyPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: company, isPending, isError, error, refetch } = useCompanyQuery(id);
  const { apiError, mutateAsync } = useUpdateCompanyMutation(id);

  const initial = useMemo<CompanyFormValues>(() => {
    if (!company) return defaultFormCompany;
    return companyFormValuesFromDto(company);
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
            <CompanyFormFields submitLabel="Salvar" />
          </FormProvider>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
