'use client';

import { Alert, ApiQueryBoundary, FormFieldsSkeleton } from '@/components';
import { FormProvider } from '@/context';
import {
    defaultFormJob,
    jobFormSchema,
    jobFormValuesFromDto,
    useCloseJobMutation,
    useJobQuery,
    useUpdateJobMutation,
    type JobFormValues
} from '@/services';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { JobFormFields } from '../job-form';

export function RecruitmentEditJobPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const { apiError: updateApiError, mutateAsync: updateAsync, isPending: isUpdating } = useUpdateJobMutation(jobId);
  const { apiError: closeApiError, mutateAsync: closeAsync, isPending: isClosing, error: closeError, isError: isCloseError } = useCloseJobMutation(jobId);
  const apiError = updateApiError ?? closeApiError;

  const initial = useMemo<JobFormValues>(() => {
    if (!job) return defaultFormJob;
    return jobFormValuesFromDto(job);
  }, [job]);

  const handleSubmit = async (formValue: JobFormValues) => await updateAsync(formValue);
  const onClose = () => void closeAsync();

  return (
    <ApiQueryBoundary
      fallback="vaga"
      isPending={isPending || isClosing}
      isError={isError || isCloseError}
      error={error || closeError}
      resource="vaga"
      onRetry={() => void refetch()}
    >
      <section>
        <h1>Editar vaga</h1>
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? (
          <FormFieldsSkeleton fields={8} />
        ) : (
          <FormProvider
            key={`job-${jobId}`}
            validationSchema={jobFormSchema}
            defaultValues={initial}
            onSubmit={handleSubmit}
          >
            <JobFormFields
              submitLabel="Salvar"
              onClose={onClose}
              closeDisabled={isUpdating || isClosing}
            />
          </FormProvider>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
