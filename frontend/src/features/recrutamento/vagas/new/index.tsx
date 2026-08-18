'use client';

import { Alert, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import { JobFormFields, defaultFormJob, jobFormSchema, type JobFormValues } from '../form';
import { jobsRoutes } from '../jobs-routes';
import { useCreateJobMutation } from '../service';

export function RecruitmentNewJobPage() {
  const { apiError, mutateAsync } = useCreateJobMutation();
  const handleSubmit = async (formValue: JobFormValues) => await mutateAsync(formValue);

  return (
    <div>
      <PageHeader title="Nova vaga" description="Publique uma nova vaga para a sua empresa." />
      {apiError ? (
        <Alert variant="destructive" title="Erro">
          {apiError}
        </Alert>
      ) : null}

      <FormProvider validationSchema={jobFormSchema} defaultValues={defaultFormJob} onSubmit={handleSubmit}>
        <JobFormFields submitLabel="Criar vaga" backHref={jobsRoutes.list} />
      </FormProvider>
    </div>
  );
}
