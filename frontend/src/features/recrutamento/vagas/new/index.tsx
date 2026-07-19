'use client';

import { Alert, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import { defaultFormJob, jobFormSchema, useCreateJobMutation, type JobFormValues } from '../service';
import { JobFormFields } from '../job-form';

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
        <JobFormFields submitLabel="Criar vaga" />
      </FormProvider>
    </div>
  );
}
