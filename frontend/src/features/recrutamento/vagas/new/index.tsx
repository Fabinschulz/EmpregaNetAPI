'use client';

import { Alert } from '@/components';
import { FormProvider } from '@/context';
import {
  defaultFormJob,
  jobFormSchema,
  useCreateJobMutation,
  type JobFormValues
} from '@/services';
import { JobFormFields } from '../job-form';

export function RecruitmentNewJobPage() {
  const { apiError, mutateAsync } = useCreateJobMutation();
  const handleSubmit = async (formValue: JobFormValues) => await mutateAsync(formValue);

  return (
    <div>
      <h1>Nova vaga</h1>
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
