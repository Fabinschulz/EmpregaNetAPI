'use client';

import { Alert, ApiQueryBoundary, Button, FormFieldsSkeleton, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import { Users } from 'lucide-react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { JobFormFields } from '../job-form';
import {
  defaultFormJob,
  jobFormSchema,
  jobFormValuesFromDto,
  useCloseJobMutation,
  useJobQuery,
  useUpdateJobMutation,
  type JobFormValues
} from '../service';

export function RecruitmentEditJobPage() {
  const params = useParams<{ id: string }>();
  const jobId = useMemo(() => Number(params.id), [params.id]);
  const { data: job, isPending, isError, error, refetch } = useJobQuery(jobId);
  const { apiError: updateApiError, mutate: update, isPending: isUpdating } = useUpdateJobMutation(jobId);
  const { apiError: closeApiError, mutate: close, isPending: isClosing } = useCloseJobMutation(jobId);
  const apiError = updateApiError ?? closeApiError;

  const initial = useMemo<JobFormValues>(() => {
    if (!job) return defaultFormJob;
    return jobFormValuesFromDto(job);
  }, [job]);

  const handleSubmit = (formValue: JobFormValues) => update(formValue);

  return (
    <ApiQueryBoundary
      fallback="vaga"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="vaga"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader
          title="Editar vaga"
          description="Atualize os dados ou encerre a vaga."
          actions={
            <Button variant="outline" asChild>
              <Link href={`/recrutamento/vagas/${jobId}/candidatos`}>
                <Users aria-hidden />
                Ver candidatos
              </Link>
            </Button>
          }
        />
        {apiError ? (
          <Alert variant="destructive" title="Erro" style={{ margin: '1rem 0' }}>
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
            <JobFormFields submitLabel="Salvar" onClose={close} closeDisabled={isUpdating || isClosing} />
          </FormProvider>
        )}
      </section>
    </ApiQueryBoundary>
  );
}
