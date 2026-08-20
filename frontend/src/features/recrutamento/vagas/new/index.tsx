'use client';

import { Alert, FormHeader, FormNotice } from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { JobFormFields, defaultFormJob, jobFormSchema, type JobFormValues } from '../form';
import { jobsRoutes } from '../jobs-routes';
import { useCreateJobMutation } from '../service';

export function RecruitmentNewJobPage() {
  const { apiError, mutateAsync } = useCreateJobMutation();
  const handleSubmit = async (formValue: JobFormValues) => await mutateAsync(formValue);

  return (
    <FormProvider validationSchema={jobFormSchema} defaultValues={defaultFormJob} onSubmit={handleSubmit}>
      <FormHeader
        title="Nova vaga"
        description="Publique uma nova vaga para a sua empresa."
        backHref={jobsRoutes.list}
        submitLabel="Criar vaga"
      />
      {apiError ? (
        <FormNotice>
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        </FormNotice>
      ) : null}
      <JobFormFields />
    </FormProvider>
  );
}
